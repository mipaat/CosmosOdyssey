using Domain.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity;

public class User : IdentityUser<Guid>, IIdDatabaseEntity
{
    public ICollection<UserRole>? UserRoles { get; set; }
    public ICollection<UserClaim>? UserClaims { get; set; }
    public ICollection<UserLogin>? UserLogins { get; set; }
    public ICollection<UserToken>? UserTokens { get; set; }
}