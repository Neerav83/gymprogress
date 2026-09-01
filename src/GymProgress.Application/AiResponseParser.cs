using System.Text.Json;
using GymProgress.Application.Contracts;

namespace GymProgress.Application;

public static class AiResponseParser
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
    };

    public static AiWorkoutRecommendation ParseRecommendation(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new CoachInvalidResponseException("Coachen svarade utan innehåll.");
        }

        var json = Unwrap(content);
        try
        {
            var parsed = JsonSerializer.Deserialize<AiWorkoutRecommendation>(json, JsonOptions);
            if (parsed is null)
            {
                throw new CoachInvalidResponseException("Coachen svarade med tom JSON.");
            }

            return parsed;
        }
        catch (JsonException exception)
        {
            throw new CoachInvalidResponseException("Coachen svarade inte med giltig JSON.", exception);
        }
    }

    public static string Unwrap(string content)
    {
        var trimmed = content.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var start = trimmed.IndexOf('\n');
        var end = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        if (start < 0 || end <= start)
        {
            return trimmed;
        }

        return trimmed[(start + 1)..end].Trim();
    }
}
