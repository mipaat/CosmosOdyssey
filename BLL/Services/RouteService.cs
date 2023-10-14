using System.Linq.Expressions;
using System.Security.Principal;
using BLL.DTO.Entities;
using BLL.DTO.Mappers;
using DAL.EF.DbContexts;
using DAL.EF.Extensions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Utils;

namespace BLL.Services;

public class RouteService
{
    private readonly AbstractAppDbContext _ctx;

    public RouteService(AbstractAppDbContext ctx)
    {
        _ctx = ctx;
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

        SortBehaviour<LegProvider> sortOptions =
            search.SortBy.Name switch
            {
                nameof(LegProviderSummary.Price) => (e => e.Price, false),
                nameof(LegProviderSummary.Distance) => (e => e.Leg!.DistanceKm, false),
                nameof(LegProviderSummary.TravelTime) => (e => e.Arrival - e.Departure, false),
                nameof(LegProviderSummary.Departure) => (e => e.Departure, false),
                nameof(LegProviderSummary.Arrival) => (e => e.Arrival, false),
                _ => (e => e.Arrival - e.Departure, false),
            };

        query = query.OrderBy(search.SortBy, sortOptions);

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

        throw new NotImplementedException();
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
        shouldRoughMatch = false; // TODO: remove debug override
        if (shouldRoughMatch)
        {
            var names = currentSourceNodes.Values
                .Select(e => PostgresUtils.GetContainsPattern(e.Name));
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
            var names = currentSourceNodes.Values.Select(e => e.Name).ToList();
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

    private class RouteSearchState
    {
        public readonly Dictionary<string, LocationNode> AllNodes = new();
        public readonly Dictionary<LocationNode, List<Path>?> StartingNodes = new();
        public readonly Dictionary<Guid, LegEdge> AllEdges = new();

        public readonly RouteSearchDirectionSpecificState Forward = new();
        public readonly RouteSearchDirectionSpecificState Backward = new();

        internal class RouteSearchDirectionSpecificState
        {
            public readonly HashSet<Guid> SeenLegIds = new();
            public readonly HashSet<string> SeenNodeNames = new();
            public readonly Dictionary<string, LocationNode> CurrentNodes = new();
        }
    }

    private class LocationNode
    {
        public LocationNode(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public ICollection<LegEdge> Outgoing { get; set; } = new List<LegEdge>();
        public ICollection<LegEdge> Incoming { get; set; } = new List<LegEdge>();

        public List<Path> FindOutgoingPaths(
            string target,
            Action<Path> relevantPathSegmentFoundCallback,
            LocationNode? from = null,
            Stack<LocationNode>? ignore = null)
        {
            from ??= this;
            ignore ??= new Stack<LocationNode>();
            ignore.Push(this);
            var result = new List<Path>();

            foreach (var edge in Outgoing)
            {
                if (ignore.Contains(edge.To))
                {
                    continue;
                }

                if (edge.To.Name.Contains(target, StringComparison.OrdinalIgnoreCase))
                {
                    var path = new Path(edge.From, edge.To, edge);
                    relevantPathSegmentFoundCallback(path);
                    result.Add(path);
                }

                foreach (var nextPath in edge.To.FindOutgoingPaths(target, relevantPathSegmentFoundCallback, from, ignore))
                {
                    var path = new Path(this, nextPath.To, edge, nextPath);
                    relevantPathSegmentFoundCallback(path);
                    result.Add(path);
                }
            }

            ignore.Pop();

            return result;
        }
    }

    private class Path
    {
        public Path(LocationNode from, LocationNode to, LegEdge edge, Path? next = null)
        {
            if (to.Name != edge.To.Name)
            {
                if (next == null)
                {
                    throw new ArgumentException(
                        $"Path leading to '{to.Name}' can't end with edge leading to '{edge.To.Name}'");
                }

                if (next.From.Name != edge.To.Name)
                {
                    throw new ArgumentException(
                        $"Path's edge's target '{edge.To.Name}' doesn't match next path source '{next.From.Name}'");
                }
            }

            From = from;
            To = to;
            Edge = edge;
            Next = next;
        }

        public LocationNode From { get; }
        public LocationNode To { get; }
        public LegEdge Edge { get; set; }
        public Path? Next { get; set; }
    }

    private class LegEdge
    {
        public LocationNode From { get; set; }
        public LocationNode To { get; set; }
        public Leg Leg { get; set; }

        public LegEdge(Leg leg, LocationNode from, LocationNode to)
        {
            if (leg.StartLocation!.Name != from.Name)
            {
                throw new ArgumentException(
                    $"Start node name '{from.Name}' didn't match leg start '{leg.StartLocation.Name}'", nameof(from));
            }

            if (leg.EndLocation!.Name != to.Name)
            {
                throw new ArgumentException(
                    $"End node name '{to.Name}' didn't match leg start '{leg.EndLocation.Name}'", nameof(to));
            }

            Leg = leg;
            From = from;
            To = to;
            from.Outgoing.Add(this);
            to.Incoming.Add(this);
        }
    }
}