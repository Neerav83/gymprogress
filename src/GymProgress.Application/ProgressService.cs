using GymProgress.Application.Contracts;
using GymProgress.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymProgress.Application;

public sealed class ProgressService(IApplicationDbContext db, ICurrentUser currentUser, WorkoutService workouts)
{
    public async Task<ExerciseProgressDto?> GetAsync(Guid exerciseId, string range, CancellationToken cancellationToken)
    {
        var exercise = await db.Exercises.AsNoTracking().FirstOrDefaultAsync(item => item.Id == exerciseId, cancellationToken);
        if (exercise is null)
        {
            return null;
        }

        var since = ResolveSince(range);
        var sets = await db.WorkoutSets
            .AsNoTracking()
            .Where(set =>
                set.WorkoutExercise.ExerciseId == exerciseId &&
                set.WorkoutExercise.Workout.UserId == currentUser.UserId &&
                (since == null || set.WorkoutExercise.Workout.StartedAt >= since))
            .Select(set => new
            {
                set.WeightKg,
                set.Reps,
                Date = set.WorkoutExercise.Workout.StartedAt,
                set.CompletedAt
            })
            .ToListAsync(cancellationToken);

        var points = sets
            .GroupBy(set => DateOnly.FromDateTime(set.Date.UtcDateTime.Date))
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var maxWeight = group.Max(set => set.WeightKg);
                var totalReps = group.Sum(set => set.Reps);
                var volume = group.Sum(set => StrengthMetrics.Volume(set.WeightKg, set.Reps));
                var estimated = group.Max(set => StrengthMetrics.EstimatedOneRepMax(set.WeightKg, set.Reps));
                var timestamp = group.Min(set => set.Date);
                return new ProgressPointDto(timestamp, maxWeight, totalReps, volume, estimated);
            })
            .ToList();

        var records = await PersonalRecordCalculator.ListAsync(db, currentUser.UserId, exerciseId, cancellationToken);

        return new ExerciseProgressDto(exercise.Id, exercise.Name, NormalizeRange(range), points, records);
    }

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var active = await workouts.GetActiveAsync(cancellationToken);
        var recent = (await workouts.ListAsync(cancellationToken)).Take(8).ToList();
        var records = (await PersonalRecordCalculator.ListAsync(db, currentUser.UserId, null, cancellationToken))
            .OrderByDescending(record => record.AchievedAt)
            .Take(6)
            .ToList();

        var weekStart = DateTimeOffset.UtcNow.AddDays(-7);
        var workoutsThisWeek = await db.Workouts.CountAsync(
            workout => workout.UserId == currentUser.UserId && workout.StartedAt >= weekStart,
            cancellationToken);

        return new DashboardDto(active, recent, records, workoutsThisWeek);
    }

    public Task<IReadOnlyList<PersonalRecordDto>> ListRecordsAsync(
        Guid? exerciseId,
        CancellationToken cancellationToken) =>
        PersonalRecordCalculator.ListAsync(db, currentUser.UserId, exerciseId, cancellationToken);

    private static DateTimeOffset? ResolveSince(string? range) => NormalizeRange(range) switch
    {
        "7d" => DateTimeOffset.UtcNow.AddDays(-7),
        "30d" => DateTimeOffset.UtcNow.AddDays(-30),
        "3m" => DateTimeOffset.UtcNow.AddMonths(-3),
        "6m" => DateTimeOffset.UtcNow.AddMonths(-6),
        _ => null
    };

    private static string NormalizeRange(string? range) => range?.Trim().ToLowerInvariant() switch
    {
        "7d" or "7" => "7d",
        "30d" or "30" => "30d",
        "3m" or "90d" => "3m",
        "6m" or "180d" => "6m",
        _ => "all"
    };
}
