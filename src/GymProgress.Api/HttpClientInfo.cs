using GymProgress.Application;

namespace GymProgress.Api;

public sealed class HttpClientInfo(IHttpContextAccessor accessor) : IClientInfo
{
    public string? GetIpAddress()
    {
        return accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }
}
