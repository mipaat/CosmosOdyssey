using BLL.DTO.Entities;
using BLL.Services;
using DAL.EF.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels.Reservations;

namespace WebApp.Controllers;

public class ReservationsController : Controller
{
    private readonly RouteService _routeService;
    private readonly ReservationService _reservationService;
    private readonly AbstractAppDbContext _ctx;

    public ReservationsController(RouteService routeService, ReservationService reservationService, AbstractAppDbContext ctx)
    {
        _routeService = routeService;
        _reservationService = reservationService;
        _ctx = ctx;
    }

    [ActionName("Create")]
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> CreateGet(Guid legProviderId)
    {
        if (legProviderId == default)
        {
            return RedirectToAction("Index", "Route");
        }
        return View(await PrepareCreateGetModel(new CreateGetModel { LegProviderId = legProviderId }));
    }

    [ActionName("Create")]
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> PostReservation([FromForm] CreatePostModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(await PrepareCreateGetModel(new CreateGetModel
            {
                LegProviderId = model.LegProviderId,
                FirstName = model.FirstName,
                LastName = model.LastName
            }));
        }

        var result = await _reservationService.CreateReservation(model.LegProviderId, model.FirstName, model.LastName, User);
        switch (result.Type)
        {
            case EReservationResultType.NotFound:
                return NotFound();
            case EReservationResultType.Expired:
                ModelState.AddModelError(string.Empty, "Sorry, this reservation can't be made because it contains an expired offer!");
                return View(await PrepareCreateGetModel(new CreateGetModel
                {
                    LegProviderId = model.LegProviderId,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                }));
            case EReservationResultType.Success:
                await _ctx.SaveChangesAsync();
                return RedirectToAction("Details", new
                {
                    Id = result.ReservationId,
                    showSuccessMessage = true
                });
            default:
                throw new ApplicationException($"Unexpected reservation result type {result.Type}");
        }
    }

    public async Task<IActionResult> Details(Guid id, bool showSuccessMessage = false)
    {
        return View(new DetailsModel
        {
            Id = id,
            ShowSuccessMessage = showSuccessMessage,
        });
    }

    private async Task<CreateGetModel> PrepareCreateGetModel(CreateGetModel model)
    {
        var providers = await _routeService.GetLegProvidersByIds(model.LegProviderId);
        model.LegProviders = providers;
        model.TotalTravelTime = providers.Max(e => e.Arrival) - providers.Min(e => e.Departure);
        model.TotalPrice = providers.Sum(e => e.Price);
        return model;
    }
}