using System.Text.Json;
using DAL.EF.DbContexts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.CosmosOdyssey.Services;

public class PriceListService
{
    private const string ApiUrl = "https://cosmos-odyssey.azurewebsites.net/api/v1.0/TravelPrices";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly AbstractAppDbContext _ctx;
    private readonly ILogger<PriceListService> _logger;

    public PriceListService(AbstractAppDbContext ctx, ILogger<PriceListService> logger)
    {
        _ctx = ctx;
        _logger = logger;
    }

    public static async Task<DTO.PriceList> FetchPriceList()
    {
        using var httpClient = new HttpClient();
        await using var responseStream = await httpClient.GetStreamAsync(ApiUrl);
        var priceList = await JsonSerializer.DeserializeAsync<DTO.PriceList>(responseStream, Options);
        if (priceList == null)
        {
            throw new ApplicationException("Failed to fetch price list");
        }

        if (priceList.ValidUntil <= DateTime.UtcNow)
        {
            throw new ApplicationException(
                $"Fetched price list was invalid! {nameof(priceList.ValidUntil)}: {priceList.ValidUntil}, current time: {DateTime.UtcNow}");
        }

        return priceList;
    }

    public async Task AddPriceList(DTO.PriceList source)
    {
        if (await _ctx.PriceLists.Where(e => e.ValidUntil > DateTime.UtcNow && e.ExternalId == source.Id).AnyAsync())
        {
            _logger.LogInformation("Not updating price list, because a price list with ID {ID} already exists",
                source.Id);
            return;
        }

        var priceList = new PriceList
        {
            ExternalId = source.Id,
            ValidUntil = source.ValidUntil,
        };
        _ctx.Add(priceList);

        AddLegs(source, priceList);
    }

    private void AddLegs(DTO.PriceList source, PriceList priceList)
    {
        var seenLocations = new Dictionary<Guid, Location>();
        var seenCompanies = new Dictionary<Guid, Company>();
        
        foreach (var sourceLeg in source.Legs)
        {
            var leg = new Leg
            {
                ExternalId = sourceLeg.Id,
                RouteInfoExternalId = sourceLeg.RouteInfo.Id,
                DistanceKm = sourceLeg.RouteInfo.Distance,

                PriceList = priceList,
                // Note: For some reason each location in the API responses has a different ID, even if they have the same name.
                // Since the API treats them as different, so will we. (Also because all the API-provided details need to be stored.)
                StartLocation = GetLocation(sourceLeg.RouteInfo.From, seenLocations),
                EndLocation = GetLocation(sourceLeg.RouteInfo.To, seenLocations)
            };

            _ctx.Add(leg);

            AddLegProviders(sourceLeg, leg, seenCompanies);
        }
    }

    private void AddLegProviders(DTO.Leg sourceLeg, Leg leg, Dictionary<Guid, Company> seenCompanies)
    {
        foreach (var provider in sourceLeg.Providers)
        {
            var legProvider = new LegProvider
            {
                ExternalId = provider.Id,
                Price = provider.Price,
                Departure = provider.FlightStart,
                Arrival = provider.FlightEnd,

                Leg = leg,
                Company = GetCompany(provider.Company, seenCompanies),
            };

            _ctx.Add(legProvider);
        }
    }

    private static Company GetCompany(DTO.Company source, IDictionary<Guid, Company> seenCompanies)
    {
        if (!seenCompanies.TryGetValue(source.Id, out var target))
        {
            target = new Company
            {
                ExternalId = source.Id,
                Name = source.Name,
            };
            seenCompanies.Add(target.ExternalId, target);
        }

        return target;
    }

    private static Location GetLocation(DTO.Location source, IDictionary<Guid, Location> seenLocations)
    {
        if (!seenLocations.TryGetValue(source.Id, out var target))
        {
            target = new Location
            {
                ExternalId = source.Id,
                Name = source.Name,
            };
            seenLocations.Add(target.ExternalId, target);
        }

        return target;
    }
}