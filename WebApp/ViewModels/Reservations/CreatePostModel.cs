using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Reservations;

public class CreatePostModel
{
    [Required]
    [MinLength(1)]
    public List<Guid>? LegProviderIds { get; set; } = default!;
    [Required]
    [DisplayName("First name")]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [DisplayName("Last name")]
    public string LastName { get; set; } = string.Empty;
}