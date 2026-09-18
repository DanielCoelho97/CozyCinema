using System.Security.Claims;

namespace CozyCinema.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal principal, out Guid userId)
    {
        var userIdClaim = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");

        return Guid.TryParse(userIdClaim, out userId);
    }
}
