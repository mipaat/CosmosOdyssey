using BLL.DTO.Entities;
using BLL.DTO.Mappers;
using DAL.EF.DbContexts;
using DAL.EF.Extensions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Utils;

namespace BLL.Services;

public partial class RouteService
{
    private readonly AbstractAppDbContext _ctx;
    private readonly ILogger<RouteService> _logger;

    public RouteService(AbstractAppDbContext ctx, ILogger<RouteService> logger)
    {
        _ctx = ctx;
        _logger = logger;
    }

    public Task<List<LegProviderSummary>> GetLegProvidersByIds(params Guid[] ids)
    {
        return _ctx.LegProviders.Where(e => ids.Contains(e.Id)).ProjectToSummary().ToListAsync();
    }

    public Task<List<LegProviderSummary>> Search(ILegProviderQuery search)
    {
        IQueryable<LegProvider> query = _ctx.LegProviders;

        query = query.WhereValid();

        if (!string.IsNullOrWhiteSpace(search.From))
        {
            var pattern = PostgresUtils.GetContainsPattern(search.From);
            query = query.Where(e =>
                EF.Functions.ILike(e.Leg!.StartLocation!.Name, pattern));
        }

        if (!string.IsNullOrWhiteSpace(search.To))
        {
            var pattern = PostgresUtils.GetContainsPattern(search.To);
            query = query.Where(e =>
                EF.Functions.ILike(e.Leg!.EndLocation!.Name, pattern));
        }

        if (!string.IsNullOrWhiteSpace(search.Company))
        {
            var pattern = PostgresUtils.GetContainsPattern(search.Company);
            query = query.Where(e =>
                EF.Functions.ILike(e.Company!.Name, pattern));
        }

        SortBehaviour<LegProvider> sortBehaviour =
            search.SortBy.Name switch
            {
                nameof(LegProviderSummary.Price) => (e => e.Price, false),
                nameof(LegProviderSummary.Distance) => (e => e.Leg!.DistanceKm, false),
                nameof(LegProviderSummary.TravelTime) => (e => e.Arrival - e.Departure, false),
                nameof(LegProviderSummary.Departure) => (e => e.Departure, false),
                nameof(LegProviderSummary.Arrival) => (e => e.Arrival, false),
                _ => (e => e.Arrival - e.Departure, false),
            };

        query = query.OrderBy(search.SortBy, sortBehaviour);

        query = query.Paginate(search);

        return query.ProjectToSummary().ToListAsync();
    }

    public async Task<List<LegProviderSummary>> SearchRoutesFromTo(ILegProviderQuery search)
    {
        if (string.IsNullOrWhiteSpace(search.From))
        {
            throw new ArgumentException("Complex route search requires source to be defined");
        }

        if (string.IsNullOrWhiteSpace(search.To))
        {
            throw new ArgumentException("Complex route search requires destination to be defined");
        }

        var (state, legIds) = await FindConnectedLegs(search.From, search.To);

        var summaryCombinations = await GetSummaryCombinations(legIds, state);

        var summaries = GetCombinedSummaries(summaryCombinations, search);

        SortBehaviour<LegProviderSummary> sortBehaviour = search.SortBy.Name switch
        {
            nameof(LegProviderSummary.Price) => (e => e.Price, false),
            nameof(LegProviderSummary.Distance) => (e => e.Distance, false),
            nameof(LegProviderSummary.TravelTime) => (e => e.Arrival - e.Departure, false),
            nameof(LegProviderSummary.Departure) => (e => e.Departure, false),
            nameof(LegProviderSummary.Arrival) => (e => e.Arrival, false),
            _ => (e => e.Arrival - e.Departure, false),
        };

        search.Total = summaries.Count;

        summaries = summaries
            .OrderBy(search.SortBy, sortBehaviour)
            .Paginate(search)
            .ToList();

        return summaries;
    }

    private static List<LegProviderSummary> GetCombinedSummaries(List<List<LegProviderSummary>> summaryCombinations,
        ILegProviderQuery search)
    {
        var result = new List<LegProviderSummary>();
        foreach (var summaryCombination in summaryCombinations)
        {
            var count = summaryCombination.Count;

            if (count == 1)
            {
                var singleSummary = summaryCombination.First();
                if (search.Company != null &&
                    (singleSummary.CompanyName?.Contains(search.Company, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    continue;
                }

                result.Add(singleSummary);
                continue;
            }

            LegProviderSummary? firstSummary = null;
            LegProviderSummary? lastSummary = null;
            if (count == 0)
            {
                throw new ApplicationException();
            }

            var totalDistance = 0L;
            decimal totalPrice = 0;
            var minValidUntil = DateTime.MaxValue;
            var containedCompany = search.Company == null;
            for (var i = 0; i < count; i++)
            {
                var legSummary = summaryCombination[i];
                if (i == 0)
                {
                    firstSummary = legSummary;
                }

                if (i == count - 1)
                {
                    lastSummary = legSummary;
                }

                if (!containedCompany && search.Company != null &&
                    (legSummary.CompanyName?.Contains(search.Company, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    containedCompany = true;
                }

                totalDistance += legSummary.Distance;
                totalPrice += legSummary.Price;
                if (legSummary.ValidUntil < minValidUntil)
                {
                    minValidUntil = legSummary.ValidUntil;
                }
            }

            if (!containedCompany)
            {
                continue;
            }

            var summary = new LegProviderSummary
            {
                StartLocation = firstSummary!.StartLocation,
                EndLocation = lastSummary!.EndLocation,
                Departure = firstSummary.Departure,
                Arrival = lastSummary.Arrival,
                TravelTime = lastSummary.Arrival - firstSummary.Departure,
                Distance = totalDistance,
                Price = totalPrice,
                ValidUntil = minValidUntil,
                SubLegs = summaryCombination,
            };
            result.Add(summary);
        }

        return result;
    }

    private async Task<List<List<LegProviderSummary>>> GetSummaryCombinations(
        ICollection<Guid> legIds,
        RouteSearchState state)
    {
        var summaryCombinations = new List<List<LegProviderSummary>>();

        var summaries = await _ctx.LegProviders
            .WhereValid()
            .Where(e => legIds.Contains(e.LegId))
            .ProjectToSummary()
            .ToListAsync();

        foreach (var paths in state.StartingNodes)
        {
            if (paths.Value == null)
            {
                _logger.LogWarning("The paths value for location node {LocationNodeName} was null", paths.Key.Name);
                continue;
            }

            foreach (var path in paths.Value)
            {
                summaryCombinations.AddRange(GetSummariesForPath(path, summaries));
            }
        }

        return summaryCombinations;
    }

    private List<List<LegProviderSummary>> GetSummariesForPath(Path path, List<LegProviderSummary> summaries)
    {
        var legProviders = summaries.Where(e => e.LegId == path.Edge.Leg.Id);
        var result = new List<List<LegProviderSummary>>();
        foreach (var legProvider in legProviders)
        {
            if (path.Next == null)
            {
                // Recursion base case
                result.Add(new List<LegProviderSummary>
                {
                    legProvider,
                });
            }
            else
            {
                var subsequentProviders = GetSummariesForPath(path.Next, summaries);
                foreach (var nextCombination in subsequentProviders)
                {
                    var nextSummary = nextCombination.First();
                    if (nextSummary.StartLocation != legProvider.EndLocation)
                    {
                        _logger.LogWarning(
                            "Next leg start '{NextStartLocation}' != previous leg end '{PreviousEndLocation}'" +
                            " when constructing summaries for path from '{From}' to '{To}'",
                            nextSummary.StartLocation, legProvider.EndLocation, path.From.Name, path.To.Name);
                        continue;
                    }

                    if (nextSummary.Departure < legProvider.Arrival)
                    {
                        continue;
                    }

                    var newCombination = new List<LegProviderSummary>
                    {
                        legProvider,
                    };
                    newCombination.AddRange(nextCombination);
                    result.Add(newCombination);
                }
            }
        }

        return result;
    }

    private async Task<(RouteSearchState state, List<Guid> legIds)> FindConnectedLegs(string from, string to)
    {
        var state = new RouteSearchState
        {
            Forward =
            {
                CurrentNodes =
                {
                    [from] = new LocationNode(from)
                }
            },
            Backward =
            {
                CurrentNodes =
                {
                    [to] = new LocationNode(to)
                }
            },
        };

        const int maxDepth = 3; // Arbitrarily chosen limit

        for (var i = 0; i < maxDepth; i++)
        {
            var shouldRoughMatch = i == 0;

            await IterateLegConnectionSearch(state, shouldRoughMatch, false);
            await IterateLegConnectionSearch(state, shouldRoughMatch, true);
        }

        var relevantLegIds = new List<Guid>();

        foreach (var node in state.StartingNodes.Keys)
        {
            var paths = node.FindOutgoingPaths(
                to,
                path => relevantLegIds.Add(path.Edge.Leg.Id));
            state.StartingNodes[node] = paths;
        }

        return new ValueTuple<RouteSearchState, List<Guid>>(state, relevantLegIds);
    }

    private async Task IterateLegConnectionSearch(RouteSearchState state, bool firstIteration, bool backward)
    {
        IQueryable<Leg> query = _ctx.Legs;
        query = query
            .Where(e => e.PriceList!.ValidUntil > DateTime.UtcNow)
            .Include(e => e.StartLocation)
            .Include(e => e.EndLocation);

        var directionState = backward ? state.Backward : state.Forward;

        // NB! For some reason these IDs do not get passed as parameters, but get hard-coded into the SQL query.
        // This isn't ideal, but probably won't allow SQL injection since the Guids are still Guids, not arbitrary strings?
        // Maybe that is why EF Core thinks it's OK to not pass them as parameters?
        // Still, this probably means the IDs show up in logs regardless of whether sensitive data logging is enabled?
        // TODO: Figure out how to make EF Core use parameters here.
        query = query.Where(e => !directionState.SeenLegIds.Contains(e.Id));

        query = FilterByLocationNames(query, directionState.CurrentNodes, backward, firstIteration);

        directionState.CurrentNodes.Clear();

        await foreach (var fetchedLeg in query.AsAsyncEnumerable())
        {
            directionState.SeenLegIds.Add(fetchedLeg.Id);
            var sourceNode = GetOrCreateNode(state.AllNodes, fetchedLeg.StartLocation!);
            if (firstIteration && !backward)
            {
                state.StartingNodes.TryAdd(sourceNode, null);
            }

            var targetNode = GetOrCreateNode(state.AllNodes, fetchedLeg.EndLocation!);
            if (!state.AllEdges.TryGetValue(fetchedLeg.Id, out var edge))
            {
                edge = new LegEdge(fetchedLeg, sourceNode, targetNode);
                state.AllEdges[fetchedLeg.Id] = edge;
            }

            var nextNode = backward ? sourceNode : targetNode;
            if (!directionState.SeenNodeNames.Contains(nextNode.Name))
            {
                directionState.SeenNodeNames.Add(nextNode.Name);
                directionState.CurrentNodes[nextNode.Name] = nextNode;
            }
        }
    }

    private static IQueryable<Leg> FilterByLocationNames(IQueryable<Leg> query,
        Dictionary<string, LocationNode> currentSourceNodes,
        bool backward,
        bool shouldRoughMatch)
    {
        if (shouldRoughMatch)
        {
            var names = currentSourceNodes.Values
                .Select(e => PostgresUtils.GetContainsPattern(e.Name))
                .ToList(); // NB! ToList() is required here to allow EF Core to translate this query into SQL
            query = backward
                ? query.Where(e => names
                    .Any(n => EF.Functions.ILike(
                        e.EndLocation!.Name, n)))
                : query.Where(e => names
                    .Any(n => EF.Functions.ILike(
                        e.StartLocation!.Name, n)));
        }
        else
        {
            var names = currentSourceNodes.Values
                .Select(e => e.Name)
                .ToList(); // NB! ToList() is required here to allow EF Core to translate this query into SQL
            query = backward
                ? query.Where(e => names.Contains(e.EndLocation!.Name))
                : query.Where(e => names.Contains(e.StartLocation!.Name));
        }

        return query;
    }

    private static LocationNode GetOrCreateNode(IDictionary<string, LocationNode> allNodes, Location location)
    {
        if (!allNodes.TryGetValue(location.Name, out var node))
        {
            node = new LocationNode(location.Name);
            allNodes[location.Name] = node;
        }

        return node;
    }
}