namespace GymProgress.Domain;

public sealed class WorkoutSet
{
    public Guid Id { get; set; }
    public Guid WorkoutExerciseId { get; set; }
    public WorkoutExercise WorkoutExercise { get; set; } = null!;
    public int SetNumber { get; set; }
    public decimal WeightKg { get; set; }
    public int Reps { get; set; }
    public DateTimeOffset CompletedAt { get; set; }

    public decimal Volume => StrengthMetrics.Volume(WeightKg, Reps);
    public decimal EstimatedOneRepMax => StrengthMetrics.EstimatedOneRepMax(WeightKg, Reps);
}
