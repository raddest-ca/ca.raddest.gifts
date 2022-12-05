using System.Security.Claims;

namespace Gifts.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id == null) {
            return null;
        }
        return Guid.Parse(id);
    }
}