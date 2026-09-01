using GymProgress.Application.Contracts;
using GymProgress.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymProgress.Application;

public static class PersonalRecordCalculator
{
    public static async Task<IReadOnlyList<PersonalRecordDto>> ListAsync(
        IApplicationDbContext db,
        Guid userId,
        Guid? exerciseId,
        CancellationToken cancellationToken)
    {
        var query = HistoricalSets(db, userId);
        if (exerciseId is not null)
        {
            query = query.Where(set => set.WorkoutExercise.ExerciseId == exerciseId);
        }

        var sets = await query.ToListAsync(cancellationToken);
        return BuildRecords(sets)
            .OrderByDescending(record => record.AchievedAt)
            .ToList();
    }

    public static async Task<IReadOnlyList<PersonalRecordHitDto>> DetectHitsAsync(
        IApplicationDbContext db,
        Guid userId,
        WorkoutExercise currentExercise,
        WorkoutSet currentSet,
        CancellationToken cancellationToken)
    {
        var previousSets = await HistoricalSets(db, userId)
            .Where(set =>
                set.WorkoutExercise.ExerciseId == currentExercise.ExerciseId &&
                set.Id != currentSet.Id)
            .ToListAsync(cancellationToken);

        var previous = BuildLookup(previousSets);
        var hits = new List<PersonalRecordHitDto>();

        if (previous.MaxWeightSet is not null && currentSet.WeightKg > previous.MaxWeight)
        {
            hits.Add(new PersonalRecordHitDto(
                nameof(PersonalRecordType.HighestWeight),
                $"{FormatWeight(currentSet.WeightKg)} kg",
                currentSet.WeightKg,
                currentSet.Reps,
                $"{FormatWeight(previous.MaxWeight)} kg × {previous.MaxWeightSet.Reps}"));
        }

        var previousRepsAtWeight = previousSets
            .Where(set => set.WeightKg == currentSet.WeightKg)
            .Select(set => set.Reps)
            .DefaultIfEmpty(0)
            .Max();

        if (previousRepsAtWeight > 0 && currentSet.Reps > previousRepsAtWeight)
        {
            hits.Add(new PersonalRecordHitDto(
                nameof(PersonalRecordType.MostRepsAtWeight),
                $"{FormatWeight(currentSet.WeightKg)} kg × {currentSet.Reps}",
                currentSet.WeightKg,
                currentSet.Reps,
                $"{FormatWeight(currentSet.WeightKg)} kg × {previousRepsAtWeight}"));
        }

        if (previous.MaxOneRmSet is not null && currentSet.EstimatedOneRepMax > previous.MaxOneRm)
        {
            hits.Add(new PersonalRecordHitDto(
                nameof(PersonalRecordType.HighestEstimatedOneRm),
                $"e1RM {FormatWeight(currentSet.EstimatedOneRepMax)} kg",
                currentSet.WeightKg,
                currentSet.Reps,
                $"e1RM {FormatWeight(previous.MaxOneRm)} kg"));
        }

        var currentVolume = currentExercise.Sets.Sum(set => set.Volume);
        if (previous.MaxSessionVolume > 0 && currentVolume > previous.MaxSessionVolume)
        {
            hits.Add(new PersonalRecordHitDto(
                nameof(PersonalRecordType.HighestVolume),
                $"{FormatWeight(currentVolume)} kg volym",
                currentSet.WeightKg,
                currentSet.Reps,
                $"{FormatWeight(previous.MaxSessionVolume)} kg volym"));
        }

        return hits;
    }

    private static IQueryable<WorkoutSet> HistoricalSets(IApplicationDbContext db, Guid userId) =>
        db.WorkoutSets
            .AsNoTracking()
            .Include(set => set.WorkoutExercise)
                .ThenInclude(exercise => exercise.Exercise)
            .Where(set => set.WorkoutExercise.Workout.UserId == userId);

    private static IReadOnlyList<PersonalRecordDto> BuildRecords(IReadOnlyList<WorkoutSet> sets)
    {
        return sets
            .GroupBy(set => set.WorkoutExercise.ExerciseId)
            .SelectMany(group =>
            {
                var lookup = BuildLookup(group.ToList());
                var name = group.First().WorkoutExercise.Exercise.Name;
                var records = new List<PersonalRecordDto>();

                if (lookup.MaxWeightSet is not null)
                {
                    records.Add(ToDto(
                        group.Key,
                        name,
                        PersonalRecordType.HighestWeight,
                        lookup.MaxWeightSet,
                        lookup.MaxWeight,
                        $"{FormatWeight(lookup.MaxWeight)} kg × {lookup.MaxWeightSet.Reps}"));
                }

                var bestReps = group
                    .GroupBy(set => set.WeightKg)
                    .Select(weightGroup => weightGroup.OrderByDescending(set => set.Reps).First())
                    .OrderByDescending(set => set.Reps)
                    .ThenByDescending(set => set.WeightKg)
                    .FirstOrDefault();

                if (bestReps is not null)
                {
                    records.Add(ToDto(
                        group.Key,
                        name,
                        PersonalRecordType.MostRepsAtWeight,
                        bestReps,
                        bestReps.Reps,
                        $"{FormatWeight(bestReps.WeightKg)} kg × {bestReps.Reps}"));
                }

                if (lookup.MaxOneRmSet is not null)
                {
                    records.Add(ToDto(
                        group.Key,
                        name,
                        PersonalRecordType.HighestEstimatedOneRm,
                        lookup.MaxOneRmSet,
                        lookup.MaxOneRm,
                        $"e1RM {FormatWeight(lookup.MaxOneRm)} kg"));
                }

                if (lookup.MaxSessionVolumeSet is not null)
                {
                    records.Add(ToDto(
                        group.Key,
                        name,
                        PersonalRecordType.HighestVolume,
                        lookup.MaxSessionVolumeSet,
                        lookup.MaxSessionVolume,
                        $"{FormatWeight(lookup.MaxSessionVolume)} kg volym"));
                }

                return records;
            })
            .ToList();
    }

    private static RecordLookup BuildLookup(IReadOnlyList<WorkoutSet> sets)
    {
        var maxWeightSet = sets.OrderByDescending(set => set.WeightKg).ThenByDescending(set => set.Reps).FirstOrDefault();
        var maxOneRmSet = sets.OrderByDescending(set => set.EstimatedOneRepMax).FirstOrDefault();
        var maxVolumeGroup = sets
            .GroupBy(set => set.WorkoutExercise.WorkoutId)
            .Select(group => new
            {
                Volume = group.Sum(set => set.Volume),
                Set = group.OrderByDescending(set => set.CompletedAt).First()
            })
            .OrderByDescending(group => group.Volume)
            .FirstOrDefault();

        return new RecordLookup(
            maxWeightSet?.WeightKg ?? 0,
            maxWeightSet,
            maxOneRmSet?.EstimatedOneRepMax ?? 0,
            maxOneRmSet,
            maxVolumeGroup?.Volume ?? 0,
            maxVolumeGroup?.Set);
    }

    private static PersonalRecordDto ToDto(
        Guid exerciseId,
        string name,
        PersonalRecordType type,
        WorkoutSet set,
        decimal value,
        string label) =>
        new(
            exerciseId,
            name,
            type.ToString(),
            set.WeightKg,
            set.Reps,
            value,
            set.CompletedAt,
            label);

    private static string FormatWeight(decimal weight) =>
        weight % 1 == 0 ? weight.ToString("0") : weight.ToString("0.##");

    private sealed record RecordLookup(
        decimal MaxWeight,
        WorkoutSet? MaxWeightSet,
        decimal MaxOneRm,
        WorkoutSet? MaxOneRmSet,
        decimal MaxSessionVolume,
        WorkoutSet? MaxSessionVolumeSet);
}
