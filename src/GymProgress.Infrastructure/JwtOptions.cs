namespace GymProgress.Infrastructure;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "GymProgress";
    public string Audience { get; set; } = "GymProgress";
    public string Key { get; set; } = "";
    public int ExpirationDays { get; set; } = 30;
}
