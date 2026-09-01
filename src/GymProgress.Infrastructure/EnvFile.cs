using Microsoft.Extensions.Configuration;

namespace GymProgress.Infrastructure;

public static class EnvFile
{
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["JWT_KEY"] = "Jwt__Key",
        ["AI_BASE_URL"] = "AI__BaseUrl",
        ["AI_MODEL"] = "AI__Model"
    };

    public static void LoadIntoProcessEnvironment()
    {
        if (IsSkipped())
        {
            return;
        }

        var path = Find();
        if (path is null)
        {
            return;
        }

        foreach (var (key, value) in Parse(File.ReadAllLines(path)))
        {
            SetIfMissing(key, value);
            if (Aliases.TryGetValue(key, out var mapped))
            {
                SetIfMissing(mapped, value);
            }
        }
    }

    public static string[] CorsOrigins(IConfiguration configuration)
    {
        var fromEnv = configuration["CORS_ORIGINS"];
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        return configuration.GetSection("Cors:Origins").Get<string[]>()
            ?? ["http://localhost:4200", "http://127.0.0.1:4200"];
    }

    private static bool IsSkipped()
    {
        var skip = Environment.GetEnvironmentVariable("GYMPROGRESS_SKIP_ENV_FILE");
        if (string.Equals(skip, "true", StringComparison.OrdinalIgnoreCase) || skip == "1")
        {
            return true;
        }

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        return string.Equals(environment, "Testing", StringComparison.OrdinalIgnoreCase);
    }

    private static string? Find()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, ".env");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static void SetIfMissing(string key, string value)
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    internal static IEnumerable<KeyValuePair<string, string>> Parse(IEnumerable<string> lines)
    {
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith("export ", StringComparison.Ordinal))
            {
                line = line["export ".Length..].Trim();
            }

            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            var key = line[..separator].Trim();
            var value = Unquote(line[(separator + 1)..].Trim());
            if (key.Length == 0)
            {
                continue;
            }

            yield return new KeyValuePair<string, string>(key, value);
        }
    }

    private static string Unquote(string value)
    {
        if (value.Length >= 2 &&
            ((value.StartsWith('"') && value.EndsWith('"')) || (value.StartsWith('\'') && value.EndsWith('\''))))
        {
            return value[1..^1];
        }

        return value;
    }
}
