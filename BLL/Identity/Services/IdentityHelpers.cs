using System.Security.Claims;

namespace BLL.Identity.Services;

public static class IdentityHelpers
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        return Guid.Parse(
            user.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
    }

    public static Guid? GetUserIdIfExists(this ClaimsPrincipal? user)
    {
        var stringId = user?.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return stringId == null ? null : Guid.Parse(stringId);
    }
}