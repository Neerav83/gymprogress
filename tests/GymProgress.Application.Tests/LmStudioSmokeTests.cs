using GymProgress.Application;
using GymProgress.Application.Contracts;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GymProgress.Application.Tests;

public class LmStudioSmokeTests
{
    [Fact(Skip = "Manuellt: starta LM Studio, ladda en modell och ta bort Skip (eller kör med LMSTUDIO_SMOKE=1 efter att Skip tagits bort).")]
    public async Task Recommend_against_running_lm_studio()
    {
        var options = Options.Create(new Infrastructure.AiOptions
        {
            BaseUrl = Environment.GetEnvironmentVariable("AI_BASE_URL") ?? "http://localhost:1234",
            Model = Environment.GetEnvironmentVariable("AI_MODEL") ?? "",
            TimeoutSeconds = 90
        });

        using var http = new HttpClient
        {
            BaseAddress = new Uri(Normalize(options.Value.BaseUrl)),
            Timeout = TimeSpan.FromSeconds(90)
        };

        var coach = new Infrastructure.LmStudioCoach(http, options, NullLogger<Infrastructure.LmStudioCoach>.Instance);
        var chestId = Guid.Parse("aaaaaaaa-0000-4000-8000-000000000010");
        var context = new CoachContextDto(
            "2026-09-01",
            [
                new CoachRecentWorkoutDto("2026-08-30", [
                    new CoachRecentExerciseDto(
                        chestId.ToString(),
                        "Chest Press",
                        ["chest"],
                        [new CoachRecentSetDto(25, 10), new CoachRecentSetDto(25, 10), new CoachRecentSetDto(25, 5)])
                ])
            ],
            [
                new CoachAvailableExerciseDto(chestId.ToString(), "Chest Press", ["chest"], "Machine")
            ]);

        var result = await coach.RecommendAsync(context, CancellationToken.None);
        Assert.False(string.IsNullOrWhiteSpace(result.WorkoutType));
        Assert.NotEmpty(result.Exercises);
    }

    private static string Normalize(string baseUrl)
    {
        var value = baseUrl.Trim().TrimEnd('/');
        if (!value.EndsWith("/v1", StringComparison.OrdinalIgnoreCase))
        {
            value += "/v1";
        }

        return value + "/";
    }
}
