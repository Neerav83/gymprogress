using GymProgress.Domain;

namespace GymProgress.Application.Contracts;

public sealed record ExerciseDto(
    Guid Id,
    string Name,
    IReadOnlyList<string> MuscleGroups,
    string Equipment);

public sealed record SetDto(
    Guid Id,
    int SetNumber,
    decimal WeightKg,
    int Reps,
    decimal VolumeKg,
    decimal EstimatedOneRepMax,
    DateTimeOffset CompletedAt);

public sealed record LastSessionDto(
    DateTimeOffset PerformedAt,
    IReadOnlyList<SetDto> Sets);

public sealed record WorkoutExerciseDto(
    Guid Id,
    Guid ExerciseId,
    string ExerciseName,
    IReadOnlyList<string> MuscleGroups,
    string Equipment,
    int SortOrder,
    IReadOnlyList<SetDto> Sets,
    LastSessionDto? LastSession);

public sealed record WorkoutDto(
    Guid Id,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    bool IsActive,
    string? Notes,
    IReadOnlyList<WorkoutExerciseDto> Exercises,
    decimal TotalVolumeKg);

public sealed record WorkoutSummaryDto(
    Guid Id,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    bool IsActive,
    int ExerciseCount,
    int SetCount,
    decimal TotalVolumeKg,
    IReadOnlyList<string> ExerciseNames);

public sealed record PersonalRecordDto(
    Guid ExerciseId,
    string ExerciseName,
    string Type,
    decimal WeightKg,
    int Reps,
    decimal Value,
    DateTimeOffset AchievedAt,
    string Label);

public sealed record PersonalRecordHitDto(
    string Type,
    string Label,
    decimal WeightKg,
    int Reps,
    string? PreviousLabel);

public sealed record ProgressPointDto(
    DateTimeOffset Date,
    decimal MaxWeightKg,
    int TotalReps,
    decimal VolumeKg,
    decimal EstimatedOneRepMax);

public sealed record ExerciseProgressDto(
    Guid ExerciseId,
    string ExerciseName,
    string Range,
    IReadOnlyList<ProgressPointDto> Points,
    IReadOnlyList<PersonalRecordDto> Records);

public sealed record DashboardDto(
    WorkoutDto? ActiveWorkout,
    IReadOnlyList<WorkoutSummaryDto> RecentWorkouts,
    IReadOnlyList<PersonalRecordDto> RecentRecords,
    int WorkoutsThisWeek);

public sealed record CreateWorkoutRequest(string? Notes);

public sealed record AddExerciseRequest(Guid ExerciseId);

public sealed record AddSetRequest(decimal WeightKg, int Reps);

public sealed record UpdateSetRequest(decimal WeightKg, int Reps);

public sealed record AddSetResponse(
    SetDto Set,
    WorkoutExerciseDto Exercise,
    IReadOnlyList<PersonalRecordHitDto> PersonalRecords);

public sealed record RegisterRequest(string Email, string Password, string DisplayName);

public sealed record LoginRequest(string Email, string Password);

public sealed record UserDto(Guid Id, string Email, string DisplayName);

public sealed record AuthResponse(string Token, UserDto User);
