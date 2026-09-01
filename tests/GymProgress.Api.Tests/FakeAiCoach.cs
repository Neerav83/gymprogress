using GymProgress.Application;
using GymProgress.Application.Contracts;
using GymProgress.Infrastructure;

namespace GymProgress.Api.Tests;

public sealed class FakeAiCoach : IAiCoach
{
    public Task<AiWorkoutRecommendation> RecommendAsync(
        CoachContextDto context,
        CancellationToken cancellationToken)
    {
        var chestPressId = CatalogSeed.GuidFromName("Chest Press").ToString();
        return Task.FromResult(new AiWorkoutRecommendation(
            "Push",
            [
                new AiRecommendedExercise(chestPressId, 3, 8, 10, 25, "maintain", "Behåll 25 kg och jaga jämna reps.")
            ],
            "Ett kort push-pass utifrån din historik."));
    }
}
