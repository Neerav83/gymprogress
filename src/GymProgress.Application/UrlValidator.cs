namespace GymProgress.Application;

public static class UrlValidator
{
    private static readonly string[] AllowedSchemes = ["http", "https"];
    private static readonly string[] BlockedHosts = 
    [
        "localhost",
        "127.0.0.1",
        "0.0.0.0",
        "::1",
        "[::1]"
    ];

    public static bool IsValidAndSafeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true;
        }

        if (url.Length > 2000)
        {
            return false;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!AllowedSchemes.Contains(uri.Scheme.ToLowerInvariant()))
        {
            return false;
        }

        var host = uri.Host.ToLowerInvariant();
        
        if (BlockedHosts.Any(blocked => host.Equals(blocked, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (host.StartsWith("192.168.") || 
            host.StartsWith("10.") || 
            host.StartsWith("172.16.") ||
            host.StartsWith("172.17.") ||
            host.StartsWith("172.18.") ||
            host.StartsWith("172.19.") ||
            host.StartsWith("172.20.") ||
            host.StartsWith("172.21.") ||
            host.StartsWith("172.22.") ||
            host.StartsWith("172.23.") ||
            host.StartsWith("172.24.") ||
            host.StartsWith("172.25.") ||
            host.StartsWith("172.26.") ||
            host.StartsWith("172.27.") ||
            host.StartsWith("172.28.") ||
            host.StartsWith("172.29.") ||
            host.StartsWith("172.30.") ||
            host.StartsWith("172.31."))
        {
            return false;
        }

        return true;
    }
}
