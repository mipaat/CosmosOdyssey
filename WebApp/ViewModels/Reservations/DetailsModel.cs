using BLL.DTO.Entities;

namespace WebApp.ViewModels.Reservations;

public class DetailsModel
{
    public Guid Id { get; set; }
    public bool ShowSuccessMessage { get; set; }

    public ReservationSummary Reservation { get; set; } = default!;
}