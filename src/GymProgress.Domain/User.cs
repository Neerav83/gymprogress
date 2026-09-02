namespace GymProgress.Domain;

public sealed class User
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<Workout> Workouts { get; set; } = [];
    public ICollection<BodyMetrics> BodyMetrics { get; set; } = [];
}
