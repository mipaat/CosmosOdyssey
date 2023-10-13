using BLL.Identity.Exceptions;
using BLL.Identity.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApp.ViewModels.Account;

namespace WebApp.Controllers;

public class AccountController : Controller
{
    private readonly IServiceProvider _services;
    private UserService? _userService;
    private UserService UserService => _userService ??= _services.GetRequiredService<UserService>();

    public AccountController(IServiceProvider services)
    {
        _services = services;
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        return View(new RegisterModel
        {
            ReturnUrl = returnUrl,
        });
    }

    [HttpPost]
    [ActionName(nameof(Register))]
    public async Task<IActionResult> RegisterPost([FromForm] RegisterModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var user = await UserService.RegisterUserAsync(model.Input.UserName, model.Input.Password);

                var signInResult = await UserService.SignInIdentityCookieAsync(user, model.Input.Password,
                    model.Input.RememberMe);
                if (signInResult.Succeeded)
                {
                    return Redirect(model.ReturnUrl ?? Url.Content("~/"));
                }

                return RedirectToAction(nameof(Login));
            }
            catch (IdentityOperationFailedException e)
            {
                if (!e.Errors.Any())
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong, sorry!");
                }
                else
                {
                    foreach (var error in e.Errors)
                    {
                        if (AllowedPasswordErrors.Contains(error.Code))
                        {
                            ModelState.AddModelError(nameof(model.Input) + "." + nameof(model.Input.Password),
                                error.Description);
                        }
                        else if (AllowedRegisterUsernameErrors.Contains(error.Code))
                        {
                            ModelState.AddModelError(nameof(model.Input) + "." + nameof(model.Input.UserName),
                                error.Description);
                        }
                    }
                }
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        await HttpContext.SignOutAsync();
        return View(new LoginModel
        {
            ReturnUrl = returnUrl,
        });
    }

    [HttpPost]
    [ActionName(nameof(Login))]
    public async Task<IActionResult> LoginPost([FromForm] LoginModel model)
    {
        await HttpContext.SignOutAsync();
        if (ModelState.IsValid)
        {
            var result = await UserService.SignInIdentityCookieAsync(model.Input.UserName, model.Input.Password,
                model.Input.RememberMe);
            if (result.Succeeded)
            {
                return Redirect(model.ReturnUrl ?? Url.Content("~/"));
            }

            if (result.RequiresTwoFactor)
            {
                throw new NotImplementedException("2FA is not implemented yet");
            }
        }
        ModelState.AddModelError(string.Empty, "Invalid login attempt");

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Redirect(Url.Content("~/"));
    }

    private static readonly string[] AllowedPasswordErrors =
    {
        nameof(IdentityErrorDescriber.PasswordMismatch),
        nameof(IdentityErrorDescriber.PasswordRequiresDigit),
        nameof(IdentityErrorDescriber.PasswordRequiresLower),
        nameof(IdentityErrorDescriber.PasswordRequiresUpper),
        nameof(IdentityErrorDescriber.PasswordTooShort),
        nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric),
        nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars),
    };

    private static readonly string[] AllowedRegisterUsernameErrors =
    {
        nameof(IdentityErrorDescriber.InvalidUserName),
        nameof(IdentityErrorDescriber.DuplicateUserName),
    };
}