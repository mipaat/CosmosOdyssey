namespace BLL.Identity;

public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";

    // End of basic role names, start of combinations.

    public static readonly string[] All =
    {
        SuperAdmin,
        Admin,
    };
}