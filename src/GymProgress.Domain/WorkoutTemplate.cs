namespace GymProgress.Domain;

public sealed class WorkoutTemplate
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<WorkoutTemplateExercise> Exercises { get; set; } = [];
}
