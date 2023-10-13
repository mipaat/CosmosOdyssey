using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Account;

public class LoginModel
{
    public string? ReturnUrl { get; set; }
    public InputModel Input { get; set; } = default!;

    public class InputModel
    {
        [Required]
        [DisplayName("Username")]
        public string UserName { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [DisplayName("Remember me?")]
        public bool RememberMe { get; set; } = true;
    }
}