using GymProgress.Application.Contracts;
using GymProgress.Domain;

namespace GymProgress.Application;

public static class CoachRecommendationValidator
{
    private static readonly HashSet<string> AllowedProgression =
        new(StringComparer.OrdinalIgnoreCase) { "increase", "maintain", "decrease" };

    public static WorkoutRecommendationDto Validate(
        AiWorkoutRecommendation raw,
        IReadOnlyDictionary<Guid, Exercise> catalog)
    {
        if (raw is null)
        {
            throw new CoachInvalidResponseException("Coachen svarade inte.");
        }

        var workoutType = RequireText(raw.WorkoutType, "workoutType", 80);
        var coachNote = RequireText(raw.CoachNote, "coachNote", 1000);

        if (raw.Exercises is null || raw.Exercises.Count == 0)
        {
            throw new CoachInvalidResponseException("Rekommendationen saknar övningar.");
        }

        if (raw.Exercises.Count > 10)
        {
            throw new CoachInvalidResponseException("Rekommendationen har för många övningar.");
        }

        var seen = new HashSet<Guid>();
        var exercises = new List<WorkoutRecommendationExerciseDto>();

        foreach (var item in raw.Exercises)
        {
            if (item is null)
            {
                throw new CoachInvalidResponseException("Rekommendationen innehåller en tom övning.");
            }

            if (!Guid.TryParse(item.ExerciseId, out var exerciseId) || !catalog.TryGetValue(exerciseId, out var exercise))
            {
                throw new CoachInvalidResponseException("Rekommendationen innehåller en okänd övning.");
            }

            if (!seen.Add(exerciseId))
            {
                continue;
            }

            if (item.Sets is < 1 or > 12)
            {
                throw new CoachInvalidResponseException($"Ogiltigt antal set för {exercise.Name}.");
            }

            if (item.TargetRepsMin is < 1 or > 50 || item.TargetRepsMax is < 1 or > 50)
            {
                throw new CoachInvalidResponseException($"Ogiltigt repsintervall för {exercise.Name}.");
            }

            if (item.TargetRepsMin > item.TargetRepsMax)
            {
                throw new CoachInvalidResponseException($"Repsmin är högre än max för {exercise.Name}.");
            }

            if (item.SuggestedWeight < 0 || item.SuggestedWeight > 1000)
            {
                throw new CoachInvalidResponseException($"Ogiltig vikt för {exercise.Name}.");
            }

            if (string.IsNullOrWhiteSpace(item.Progression) || !AllowedProgression.Contains(item.Progression))
            {
                throw new CoachInvalidResponseException($"Ogiltig progression för {exercise.Name}.");
            }

            exercises.Add(new WorkoutRecommendationExerciseDto(
                exercise.Id,
                exercise.Name,
                exercise.MuscleGroups,
                exercise.Equipment.ToString(),
                item.Sets,
                item.TargetRepsMin,
                item.TargetRepsMax,
                decimal.Round(item.SuggestedWeight, 2),
                item.Progression.Trim().ToLowerInvariant(),
                RequireText(item.Reason, "reason", 400)));
        }

        if (exercises.Count == 0)
        {
            throw new CoachInvalidResponseException("Rekommendationen saknar giltiga övningar.");
        }

        return new WorkoutRecommendationDto(workoutType, coachNote, exercises);
    }

    private static string RequireText(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new CoachInvalidResponseException($"Fältet {field} saknas.");
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
