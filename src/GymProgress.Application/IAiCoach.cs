using GymProgress.Application.Contracts;

namespace GymProgress.Application;

public interface IAiCoach
{
    Task<AiWorkoutRecommendation> RecommendAsync(CoachContextDto context, CancellationToken cancellationToken);
}
