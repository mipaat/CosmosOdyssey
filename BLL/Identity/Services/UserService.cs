using BLL.Identity.Exceptions;
using Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace BLL.Identity.Services;

public class UserService
{
    private readonly IdentityUow _identityUow;

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <remarks>Doesn't require calling SaveChanges().</remarks>
    /// <param name="username">The unique username to be used for the new user account.</param>
    /// <param name="password">The password to be used for the new user account.</param>
    /// <returns>The created (BLL layer) user.</returns>
    /// <exception cref="IdentityOperationFailedException">Thrown if account creation failed - password too short, username already taken, etc.</exception>
    public async Task<User> RegisterUserAsync(string username, string password)
    {
        var user = new User
        {
            UserName = username,
        };

        var result =
            await _identityUow.UserManager.CreateAsync(
                user,
                password);
        if (!result.Succeeded)
        {
            throw new IdentityOperationFailedException(result.Errors);
        }

        return user;
    }

    public async Task<SignInResult> SignInIdentityCookieAsync(string username, string password, bool isPersistent)
    {
        var user = await _identityUow.UserManager.FindByNameAsync(username);
        return await SignInIdentityCookieAsync(user, password, isPersistent);
    }

    public async Task<SignInResult> SignInIdentityCookieAsync(User? user, string password, bool isPersistent)
    {
        return user switch
        {
            null => SignInResult.Failed,
            _ => await _identityUow.SignInManager.PasswordSignInAsync(user, password, isPersistent, false),
        };
    }

    public UserService(IdentityUow identityUow)
    {
        _identityUow = identityUow;
    }
}