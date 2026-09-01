using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using GymProgress.Application;
using GymProgress.Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GymProgress.Infrastructure;

public sealed class LmStudioCoach(
    HttpClient http,
    IOptions<AiOptions> options,
    ILogger<LmStudioCoach> logger) : IAiCoach
{
    public async Task<AiWorkoutRecommendation> RecommendAsync(
        CoachContextDto context,
        CancellationToken cancellationToken)
    {
        try
        {
            var model = await ResolveModelAsync(cancellationToken);
            var payload = BuildPayload(model, context);
            using var response = await http.PostAsJsonAsync("chat/completions", payload, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable)
            {
                throw new CoachUnavailableException("Coachen är inte tillgänglig just nu.");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                logger.LogWarning("LM Studio avvisade förfrågan ({Status}).", (int)response.StatusCode);
                throw new CoachUnavailableException("Coachen har ingen modell laddad just nu.");
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("LM Studio svarade {Status}.", (int)response.StatusCode);
                throw new CoachUnavailableException("Coachen kunde inte svara just nu.");
            }

            return ParseChatContent(body);
        }
        catch (CoachUnavailableException)
        {
            throw;
        }
        catch (CoachInvalidResponseException)
        {
            throw;
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "LM Studio-anropet timeoutade.");
            throw new CoachUnavailableException("Coachen tog för lång tid på sig.", exception);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Kunde inte nå LM Studio.");
            throw new CoachUnavailableException("Coachen är inte tillgänglig just nu.", exception);
        }
    }

    private async Task<string> ResolveModelAsync(CancellationToken cancellationToken)
    {
        var configured = options.Value.Model?.Trim();
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured;
        }

        using var response = await http.GetAsync("models", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new CoachUnavailableException("Coachen har ingen modell laddad just nu.");
        }

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var id = document.RootElement.TryGetProperty("data", out var data) && data.GetArrayLength() > 0
            ? data[0].GetProperty("id").GetString()
            : null;

        if (string.IsNullOrWhiteSpace(id))
        {
            throw new CoachUnavailableException("Coachen har ingen modell laddad just nu.");
        }

        return id;
    }

    private static object BuildPayload(string model, CoachContextDto context)
    {
        var userContent =
            "Here is the athlete's training context as JSON. Select today's workout.\n\n" +
            JsonSerializer.Serialize(context, AiResponseParser.JsonOptions);

        var schema = JsonNode.Parse(CoachJsonSchema.Schema)
            ?? throw new InvalidOperationException("JSON-schemat för coachen är ogiltigt.");

        return new
        {
            model,
            temperature = 0.2,
            max_tokens = 2048,
            messages = new object[]
            {
                new { role = "system", content = CoachPrompt.System },
                new { role = "user", content = userContent }
            },
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = CoachJsonSchema.Name,
                    strict = true,
                    schema
                }
            }
        };
    }

    private static AiWorkoutRecommendation ParseChatContent(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            var content = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return AiResponseParser.ParseRecommendation(content);
        }
        catch (CoachInvalidResponseException)
        {
            throw;
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or IndexOutOfRangeException or InvalidOperationException)
        {
            throw new CoachInvalidResponseException("Coachen gav ett ogiltigt svar.", exception);
        }
    }
}
