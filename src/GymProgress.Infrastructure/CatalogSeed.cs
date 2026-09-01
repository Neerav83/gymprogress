using GymProgress.Application;
using GymProgress.Domain;

namespace GymProgress.Infrastructure;

public static class CatalogSeed
{
    public static User DefaultUser() => new()
    {
        Id = KnownIds.DefaultUserId,
        DisplayName = "Användare",
        CreatedAt = DateTimeOffset.Parse("2026-01-01T00:00:00Z")
    };

    public static IReadOnlyList<Exercise> Exercises() =>
    [
        Exercise("Seated Leg Press", Equipment.Machine, MuscleGroup.Quads, MuscleGroup.Glutes),
        Exercise("Lat Pulldown", Equipment.Cable, MuscleGroup.Back, MuscleGroup.Biceps),
        Exercise("Chest Press", Equipment.Machine, MuscleGroup.Chest, MuscleGroup.Triceps),
        Exercise("Seated Row", Equipment.Cable, MuscleGroup.Back, MuscleGroup.Biceps),
        Exercise("Shoulder Press", Equipment.Machine, MuscleGroup.Shoulders, MuscleGroup.Triceps),
        Exercise("Leg Extension", Equipment.Machine, MuscleGroup.Quads),
        Exercise("Lying Leg Curl", Equipment.Machine, MuscleGroup.Hamstrings),
        Exercise("Hip Thrust", Equipment.Machine, MuscleGroup.Glutes),
        Exercise("Calf Raise", Equipment.Machine, MuscleGroup.Calves),
        Exercise("Cable Fly", Equipment.Cable, MuscleGroup.Chest),
        Exercise("Tricep Pushdown", Equipment.Cable, MuscleGroup.Triceps),
        Exercise("Bicep Curl", Equipment.Dumbbell, MuscleGroup.Biceps),
        Exercise("Lateral Raise", Equipment.Dumbbell, MuscleGroup.Shoulders),
        Exercise("Abdominal Crunch", Equipment.Machine, MuscleGroup.Core),
        Exercise("Squat", Equipment.Barbell, MuscleGroup.Quads, MuscleGroup.Glutes),
        Exercise("Bench Press", Equipment.Barbell, MuscleGroup.Chest, MuscleGroup.Triceps),
        Exercise("Deadlift", Equipment.Barbell, MuscleGroup.Back, MuscleGroup.Hamstrings, MuscleGroup.Glutes),
        Exercise("Romanian Deadlift", Equipment.Barbell, MuscleGroup.Hamstrings, MuscleGroup.Glutes),
        Exercise("Overhead Press", Equipment.Barbell, MuscleGroup.Shoulders, MuscleGroup.Triceps),
        Exercise("Pull-up", Equipment.Bodyweight, MuscleGroup.Back, MuscleGroup.Biceps)
    ];

    private static Exercise Exercise(string name, Equipment equipment, params string[] muscleGroups) => new()
    {
        Id = GuidFromName(name),
        Name = name,
        Equipment = equipment,
        MuscleGroups = muscleGroups
    };

    public static Guid GuidFromName(string name)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes($"gymprogress:{name}"));
        bytes[6] = (byte)((bytes[6] & 0x0f) | 0x40);
        bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);
        return new Guid(bytes.AsSpan(0, 16));
    }
}
