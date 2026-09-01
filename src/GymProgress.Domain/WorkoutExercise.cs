namespace GymProgress.Domain;

public sealed class WorkoutExercise
{
    public Guid Id { get; set; }
    public Guid WorkoutId { get; set; }
    public Workout Workout { get; set; } = null!;
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int SortOrder { get; set; }
    public ICollection<WorkoutSet> Sets { get; set; } = [];
}
