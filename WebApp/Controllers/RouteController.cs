using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels.Route;

namespace WebApp.Controllers;

public class RouteController : Controller
{
    private readonly RouteService _routeService;

    public RouteController(RouteService routeService)
    {
        _routeService = routeService;
    }

    public async Task<IActionResult> Index([FromQuery] SearchModel query)
    {
        if (!string.IsNullOrWhiteSpace(query.From) && !string.IsNullOrWhiteSpace(query.To))
        {
            query.Results = await _routeService.SearchRoutesFromTo(query);
        }
        else
        {
            query.Results = await _routeService.Search(query);
        }
        return View(query);
    }
}