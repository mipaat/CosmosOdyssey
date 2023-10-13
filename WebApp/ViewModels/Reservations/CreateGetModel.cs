using BLL.DTO.Entities;

namespace WebApp.ViewModels.Reservations;

public class CreateGetModel : CreatePostModel
{
    public List<LegProviderSummary> LegProviders { get; set; } = default!;

    public decimal TotalPrice { get; set; }
    public TimeSpan TotalTravelTime { get; set; }
}