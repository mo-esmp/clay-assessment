using System.Security.Claims;

namespace SmartLock.WebApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetId(this ClaimsPrincipal user)
    {
        var value = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.UserData)?.Value;

        return value;
    }

    public static IEnumerable<string> GetRoles(this ClaimsPrincipal user)
    {
        var values = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        return values;
    }
}