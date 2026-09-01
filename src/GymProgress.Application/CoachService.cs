using GymProgress.Application.Contracts;
using GymProgress.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GymProgress.Application;

public sealed class CoachService(IApplicationDbContext db, IAiCoach ai, ICurrentUser currentUser, ILogger<CoachService> logger)
{
    public async Task<WorkoutRecommendationDto> GetTodaysRecommendationAsync(CancellationToken cancellationToken)
    {
        var catalog = await db.Exercises.AsNoTracking().ToListAsync(cancellationToken);
        if (catalog.Count == 0)
        {
            throw new InvalidOperationException("Det finns inga övningar i katalogen.");
        }

        var context = await BuildContextAsync(catalog, cancellationToken);
        logger.LogInformation(
            "Hämtar coachrekommendation för {Date} med {WorkoutCount} tidigare pass.",
            context.Today,
            context.RecentWorkouts.Count);

        var raw = await ai.RecommendAsync(context, cancellationToken);
        var lookup = catalog.ToDictionary(exercise => exercise.Id);
        return CoachRecommendationValidator.Validate(raw, lookup);
    }

    private async Task<CoachContextDto> BuildContextAsync(
        IReadOnlyList<Exercise> catalog,
        CancellationToken cancellationToken)
    {
        var recent = await db.Workouts
            .AsNoTracking()
            .Where(workout =>
                workout.UserId == currentUser.UserId &&
                workout.FinishedAt != null &&
                workout.Exercises.Any(exercise => exercise.Sets.Any()))
            .OrderByDescending(workout => workout.StartedAt)
            .Take(12)
            .Select(workout => new
            {
                workout.StartedAt,
                Exercises = workout.Exercises
                    .OrderBy(exercise => exercise.SortOrder)
                    .Select(exercise => new
                    {
                        exercise.ExerciseId,
                        exercise.Exercise.Name,
                        exercise.Exercise.MuscleGroups,
                        Sets = exercise.Sets
                            .OrderBy(set => set.SetNumber)
                            .Select(set => new { set.WeightKg, set.Reps })
                            .ToList()
                    })
                    .Where(exercise => exercise.Sets.Count > 0)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return new CoachContextDto(
            DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            recent.Select(workout => new CoachRecentWorkoutDto(
                DateOnly.FromDateTime(workout.StartedAt.UtcDateTime).ToString("yyyy-MM-dd"),
                workout.Exercises.Select(exercise => new CoachRecentExerciseDto(
                    exercise.ExerciseId.ToString(),
                    exercise.Name,
                    exercise.MuscleGroups,
                    exercise.Sets.Select(set => new CoachRecentSetDto(set.WeightKg, set.Reps)).ToList()
                )).ToList()
            )).ToList(),
            catalog
                .OrderBy(exercise => exercise.Name)
                .Select(exercise => new CoachAvailableExerciseDto(
                    exercise.Id.ToString(),
                    exercise.Name,
                    exercise.MuscleGroups,
                    exercise.Equipment.ToString()))
                .ToList());
    }
}
