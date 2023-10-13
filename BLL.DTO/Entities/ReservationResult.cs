namespace BLL.DTO.Entities;

public class ReservationResult
{
    public EReservationResultType Type { get; set; }
    public Guid? ReservationId { get; set; }

    public static implicit operator ReservationResult(EReservationResultType type) => new() { Type = type };

    public static implicit operator ReservationResult(Guid reservationId) => new()
        { Type = EReservationResultType.Success, ReservationId = reservationId };
}