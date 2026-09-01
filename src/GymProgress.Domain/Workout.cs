namespace GymProgress.Domain;

public sealed class Workout
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public string? Notes { get; set; }
    public ICollection<WorkoutExercise> Exercises { get; set; } = [];

    public bool IsActive => FinishedAt is null;
}
