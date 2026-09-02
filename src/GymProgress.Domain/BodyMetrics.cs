namespace GymProgress.Domain;

public sealed class BodyMetrics
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset Date { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? ChestCm { get; set; }
    public decimal? WaistCm { get; set; }
    public decimal? HipsCm { get; set; }
    public decimal? ArmCm { get; set; }
    public decimal? ThighCm { get; set; }
    public string? Notes { get; set; }
    
    public User User { get; set; } = null!;
}
