namespace GymProgress.Application.Contracts;

public sealed record CoachContextDto(
    string Today,
    IReadOnlyList<CoachRecentWorkoutDto> RecentWorkouts,
    IReadOnlyList<CoachAvailableExerciseDto> AvailableExercises);

public sealed record CoachRecentWorkoutDto(
    string Date,
    IReadOnlyList<CoachRecentExerciseDto> Exercises);

public sealed record CoachRecentExerciseDto(
    string ExerciseId,
    string Name,
    IReadOnlyList<string> MuscleGroups,
    IReadOnlyList<CoachRecentSetDto> Sets);

public sealed record CoachRecentSetDto(decimal Weight, int Reps);

public sealed record CoachAvailableExerciseDto(
    string ExerciseId,
    string Name,
    IReadOnlyList<string> MuscleGroups,
    string Equipment);

public sealed record AiWorkoutRecommendation(
    string WorkoutType,
    IReadOnlyList<AiRecommendedExercise> Exercises,
    string CoachNote);

public sealed record AiRecommendedExercise(
    string ExerciseId,
    int Sets,
    int TargetRepsMin,
    int TargetRepsMax,
    decimal SuggestedWeight,
    string Progression,
    string Reason);

public sealed record WorkoutRecommendationDto(
    string WorkoutType,
    string CoachNote,
    IReadOnlyList<WorkoutRecommendationExerciseDto> Exercises);

public sealed record WorkoutRecommendationExerciseDto(
    Guid ExerciseId,
    string ExerciseName,
    IReadOnlyList<string> MuscleGroups,
    string Equipment,
    int Sets,
    int TargetRepsMin,
    int TargetRepsMax,
    decimal SuggestedWeight,
    string Progression,
    string Reason);

