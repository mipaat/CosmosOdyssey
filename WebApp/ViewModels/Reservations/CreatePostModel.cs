using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Reservations;

public class CreatePostModel
{
    public Guid LegProviderId { get; set; }
    [Required]
    [DisplayName("First name")]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [DisplayName("Last name")]
    public string LastName { get; set; } = string.Empty;
}