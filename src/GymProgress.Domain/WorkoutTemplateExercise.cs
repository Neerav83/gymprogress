namespace GymProgress.Domain;

public sealed class WorkoutTemplateExercise
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public WorkoutTemplate Template { get; set; } = null!;
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int SortOrder { get; set; }
    public int? SuggestedSets { get; set; }
    public decimal? SuggestedWeight { get; set; }
    public int? SuggestedRepsMin { get; set; }
    public int? SuggestedRepsMax { get; set; }
}
