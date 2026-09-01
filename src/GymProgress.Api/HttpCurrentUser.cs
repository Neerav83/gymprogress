using System.Security.Claims;
using GymProgress.Application;

namespace GymProgress.Api;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? accessor.HttpContext?.User.FindFirstValue("sub");
            if (!Guid.TryParse(value, out var userId))
            {
                throw new UnauthorizedAccessException("Ingen inloggad användare.");
            }

            return userId;
        }
    }
}
