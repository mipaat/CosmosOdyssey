using BLL.DTO.Entities;
using BLL.Identity.Services;
using BLL.Services;
using DAL.EF.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels.Reservations;

namespace WebApp.Controllers;

[Authorize]
public class ReservationsController : Controller
{
    private readonly RouteService _routeService;
    private readonly ReservationService _reservationService;
    private readonly AbstractAppDbContext _ctx;

    public ReservationsController(RouteService routeService, ReservationService reservationService,
        AbstractAppDbContext ctx)
    {
        _routeService = routeService;
        _reservationService = reservationService;
        _ctx = ctx;
    }

    [ActionName("Create")]
    [HttpGet]
    public async Task<IActionResult> CreateGet(List<Guid>? legProviderIds)
    {
        if (legProviderIds == null || legProviderIds.Count < 1)
        {
            return RedirectToAction("Index", "Route");
        }

        return View(await PrepareCreateGetModel(new CreateGetModel { LegProviderIds = legProviderIds }));
    }

    [ActionName("Create")]
    [HttpPost]
    public async Task<IActionResult> PostReservation([FromForm] CreatePostModel model)
    {
        if (model.LegProviderIds == null || model.LegProviderIds.Count < 1)
        {
            return RedirectToAction("Index", "Route");
        }
        if (!ModelState.IsValid)
        {
            return View(await PrepareCreateGetModel(new CreateGetModel
            {
                LegProviderIds = model.LegProviderIds,
                FirstName = model.FirstName,
                LastName = model.LastName
            }));
        }

        var result =
            await _reservationService.CreateReservation(model.LegProviderIds, model.FirstName, model.LastName, User);
        switch (result.Type)
        {
            case EReservationResultType.NotFound:
                return NotFound();
            case EReservationResultType.Expired:
                ModelState.AddModelError(string.Empty,
                    "Sorry, this reservation can't be made because it contains an expired offer!");
                return View(await PrepareCreateGetModel(new CreateGetModel
                {
                    LegProviderIds = model.LegProviderIds,
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

    public async Task<IActionResult> Index([FromQuery] IndexModel model)
    {
        var reservations = await _reservationService.GetAllForUser(User.GetUserId(), model);
        model.Reservations = reservations;
        return View(model);
    }

    [HttpGet("[controller]/{id:guid}")]
    public async Task<IActionResult> Details([FromRoute] Guid id, [FromQuery] bool showSuccessMessage = false)
    {
        var result = await _reservationService.GetByIdForUser(id, User.GetUserId());
        if (result.Type == EAccessResultType.NotAllowed)
        {
            return Forbid();
        }

        if (result.Type == EAccessResultType.NotFound || result.Result == null)
        {
            return NotFound();
        }

        return View(new DetailsModel
        {
            Id = id,
            ShowSuccessMessage = showSuccessMessage,
            Reservation = result.Result
        });
    }

    private async Task<CreateGetModel> PrepareCreateGetModel(CreateGetModel model)
    {
        if (model.LegProviderIds == null || model.LegProviderIds.Count < 1)
        {
            throw new ArgumentException($"{nameof(model.LegProviderIds)} should not be null or empty");
        }
        var providers = await _routeService.GetLegProvidersByIds(model.LegProviderIds);
        model.LegProviders = providers;
        model.TotalTravelTime = ReservationService.GetTotalTravelTime(providers);
        model.TotalPrice = ReservationService.GetTotalPrice(providers);
        return model;
    }
}