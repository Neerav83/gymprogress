using System.Reflection;

namespace GymProgress.Application;

public static class CoachPrompt
{
    public static string System { get; } = Load();

    private static string Load()
    {
        const string resource = "GymProgress.Application.Coach.GymCoachSystemPrompt.txt";
        var assembly = typeof(CoachPrompt).Assembly;
        using var stream = assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"Saknar resource {resource}.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().Trim();
    }
}
