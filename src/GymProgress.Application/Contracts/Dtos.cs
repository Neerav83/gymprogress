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

public sealed record CreateWorkoutFromRecommendationRequest(
    string WorkoutType,
    IReadOnlyList<RecommendedExerciseForWorkout> Exercises);

public sealed record RecommendedExerciseForWorkout(
    Guid ExerciseId,
    int Sets,
    decimal SuggestedWeight,
    int TargetRepsMin,
    int TargetRepsMax);

public sealed record AddExerciseRequest(Guid ExerciseId);

public sealed record AddSetRequest(decimal WeightKg, int Reps);

public sealed record UpdateSetRequest(decimal WeightKg, int Reps);

public sealed record AddSetResponse(
    SetDto Set,
    WorkoutExerciseDto Exercise,
    IReadOnlyList<PersonalRecordHitDto> PersonalRecords);

public sealed record RegisterRequest(string Email, string Password, string DisplayName);

public sealed record LoginRequest(string Email, string Password);

public sealed record UserDto(Guid Id, string Email, string DisplayName, string? ProfileImageUrl, DateTimeOffset CreatedAt);

public sealed record AuthResponse(string AccessToken, string RefreshToken, UserDto User);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record WorkoutTemplateDto(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    IReadOnlyList<WorkoutTemplateExerciseDto> Exercises);

public sealed record WorkoutTemplateExerciseDto(
    Guid ExerciseId,
    string ExerciseName,
    IReadOnlyList<string> MuscleGroups,
    string Equipment,
    int SortOrder,
    int? SuggestedSets,
    decimal? SuggestedWeight,
    int? SuggestedRepsMin,
    int? SuggestedRepsMax);

public sealed record CreateTemplateFromWorkoutRequest(
    Guid WorkoutId,
    string Name,
    string? Description);

public sealed record UpdateWorkoutTemplateRequest(
    string Name,
    string? Description,
    IReadOnlyList<Guid> ExerciseIds);

public sealed record UserProfileDto(
    Guid Id,
    string Email,
    string DisplayName,
    string? ProfileImageUrl,
    DateTimeOffset CreatedAt);

public sealed record UpdateProfileRequest(
    string? DisplayName,
    string? ProfileImageUrl);

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

public sealed record BodyMetricsDto(
    Guid Id,
    DateTimeOffset Date,
    decimal? WeightKg,
    decimal? HeightCm,
    decimal? ChestCm,
    decimal? WaistCm,
    decimal? HipsCm,
    decimal? ArmCm,
    decimal? ThighCm,
    string? Notes);

public sealed record AddBodyMetricsRequest(
    decimal? WeightKg,
    decimal? HeightCm,
    decimal? ChestCm,
    decimal? WaistCm,
    decimal? HipsCm,
    decimal? ArmCm,
    decimal? ThighCm,
    string? Notes);

public sealed record UpdateBodyMetricsRequest(
    decimal? WeightKg,
    decimal? HeightCm,
    decimal? ChestCm,
    decimal? WaistCm,
    decimal? HipsCm,
    decimal? ArmCm,
    decimal? ThighCm,
    string? Notes);

public sealed record BodyMetricsHistoryDto(
    IReadOnlyList<BodyMetricsDto> Metrics);

