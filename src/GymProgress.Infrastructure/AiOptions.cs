namespace GymProgress.Infrastructure;

public sealed class AiOptions
{
    public const string SectionName = "AI";

    public string BaseUrl { get; set; } = "http://localhost:1234";
    public string Model { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 90;
}
