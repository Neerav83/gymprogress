namespace GymProgress.Domain;

public sealed class Exercise
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string[] MuscleGroups { get; set; } = [];
    public Equipment Equipment { get; set; }
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = [];
}
