using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GymProgress.Api.Middleware;

public sealed class RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
{
    private static readonly ConcurrentDictionary<string, ClientRateLimit> _clients = new();
    private static readonly PeriodicTimer _cleanupTimer = new(TimeSpan.FromMinutes(5));

    static RateLimitingMiddleware()
    {
        Task.Run(async () =>
        {
            while (await _cleanupTimer.WaitForNextTickAsync())
            {
                var now = DateTimeOffset.UtcNow;
                var expiredKeys = _clients
                    .Where(kvp => now - kvp.Value.LastRequest > TimeSpan.FromMinutes(15))
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _clients.TryRemove(key, out _);
                }
            }
        });
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<SkipRateLimitAttribute>() != null)
        {
            await next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var rule = GetRateLimitRule(context);

        var clientLimit = _clients.GetOrAdd(clientId, _ => new ClientRateLimit());

        bool rateLimitExceeded;
        int retryAfter;

        lock (clientLimit)
        {
            var now = DateTimeOffset.UtcNow;
            
            if (now - clientLimit.WindowStart > rule.Window)
            {
                clientLimit.WindowStart = now;
                clientLimit.RequestCount = 0;
            }

            clientLimit.LastRequest = now;

            if (clientLimit.RequestCount >= rule.MaxRequests)
            {
                logger.LogWarning("Rate limit exceeded for client {ClientId} on {Path}", 
                    MaskClientId(clientId), context.Request.Path);
                
                rateLimitExceeded = true;
                retryAfter = (int)(rule.Window - (now - clientLimit.WindowStart)).TotalSeconds;
            }
            else
            {
                clientLimit.RequestCount++;
                rateLimitExceeded = false;
                retryAfter = 0;
            }
        }

        if (rateLimitExceeded)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Append("Retry-After", retryAfter.ToString());
            
            var response = new
            {
                error = "För många förfrågningar. Försök igen senare.",
                statusCode = 429,
                retryAfter
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
            
            return;
        }

        await next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userId = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            : null;

        return userId is not null ? $"user:{userId}" : $"ip:{ipAddress}";
    }

    private static string MaskClientId(string clientId)
    {
        if (clientId.StartsWith("user:"))
        {
            return $"user:***{clientId[^4..]}";
        }
        
        if (clientId.StartsWith("ip:") && clientId.Contains('.'))
        {
            var parts = clientId.Split('.');
            if (parts.Length == 4)
            {
                return $"ip:{parts[0]}.{parts[1]}.***";
            }
        }

        return "***";
    }

    private static RateLimitRule GetRateLimitRule(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        if (path.Contains("/auth/login") || path.Contains("/auth/register"))
        {
            return new RateLimitRule(5, TimeSpan.FromMinutes(1));
        }

        if (path.Contains("/auth/refresh"))
        {
            return new RateLimitRule(10, TimeSpan.FromMinutes(1));
        }

        if (path.Contains("/coach/"))
        {
            return new RateLimitRule(10, TimeSpan.FromMinutes(1));
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            return new RateLimitRule(300, TimeSpan.FromMinutes(1));
        }

        return new RateLimitRule(60, TimeSpan.FromMinutes(1));
    }

    private sealed class ClientRateLimit
    {
        public DateTimeOffset WindowStart { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset LastRequest { get; set; } = DateTimeOffset.UtcNow;
        public int RequestCount { get; set; }
    }

    private readonly record struct RateLimitRule(int MaxRequests, TimeSpan Window);
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SkipRateLimitAttribute : Attribute
{
}
