using GymProgress.Application.Contracts;
using GymProgress.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymProgress.Application;

public sealed class WorkoutService(IApplicationDbContext db, ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<WorkoutSummaryDto>> ListAsync(CancellationToken cancellationToken)
    {
        var workouts = await db.Workouts
            .AsNoTracking()
            .Where(workout => workout.UserId == currentUser.UserId)
            .Include(workout => workout.Exercises)
                .ThenInclude(exercise => exercise.Sets)
            .Include(workout => workout.Exercises)
                .ThenInclude(exercise => exercise.Exercise)
            .OrderByDescending(workout => workout.StartedAt)
            .ToListAsync(cancellationToken);

        return workouts.Select(MapSummary).ToList();
    }

    public async Task<WorkoutDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var workout = await LoadWorkoutAsync(id, cancellationToken);
        return workout is null ? null : await MapDetailAsync(workout, cancellationToken);
    }

    public async Task<WorkoutDto?> GetActiveAsync(CancellationToken cancellationToken)
    {
        var workout = await db.Workouts
            .Where(item => item.UserId == currentUser.UserId && item.FinishedAt == null)
            .Include(item => item.Exercises)
                .ThenInclude(exercise => exercise.Sets)
            .Include(item => item.Exercises)
                .ThenInclude(exercise => exercise.Exercise)
            .OrderByDescending(item => item.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return workout is null ? null : await MapDetailAsync(workout, cancellationToken);
    }

    public async Task<WorkoutDto> CreateAsync(CreateWorkoutRequest request, CancellationToken cancellationToken)
    {
        var existing = await db.Workouts
            .FirstOrDefaultAsync(
                workout => workout.UserId == currentUser.UserId && workout.FinishedAt == null,
                cancellationToken);

        if (existing is not null)
        {
            var loaded = await LoadWorkoutAsync(existing.Id, cancellationToken)
                         ?? throw new InvalidOperationException("Kunde inte läsa det pågående passet.");
            return await MapDetailAsync(loaded, cancellationToken);
        }

        var workout = new Workout
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.UserId,
            StartedAt = DateTimeOffset.UtcNow,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        db.Workouts.Add(workout);
        await db.SaveChangesAsync(cancellationToken);

        return await MapDetailAsync(workout, cancellationToken);
    }

    public async Task<WorkoutDto?> FinishAsync(Guid id, CancellationToken cancellationToken)
    {
        var workout = await LoadWorkoutAsync(id, cancellationToken);
        if (workout is null)
        {
            return null;
        }

        if (workout.FinishedAt is null)
        {
            workout.FinishedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        return await MapDetailAsync(workout, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var workout = await db.Workouts.FirstOrDefaultAsync(
            item => item.Id == id && item.UserId == currentUser.UserId,
            cancellationToken);

        if (workout is null)
        {
            return false;
        }

        db.Workouts.Remove(workout);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<WorkoutExerciseDto?> AddExerciseAsync(
        Guid workoutId,
        AddExerciseRequest request,
        CancellationToken cancellationToken)
    {
        var workout = await LoadWorkoutAsync(workoutId, cancellationToken);
        if (workout is null || !workout.IsActive)
        {
            return null;
        }

        var exercise = await db.Exercises.FirstOrDefaultAsync(item => item.Id == request.ExerciseId, cancellationToken);
        if (exercise is null)
        {
            throw new InvalidOperationException("Övningen finns inte.");
        }

        var existing = workout.Exercises.FirstOrDefault(item => item.ExerciseId == request.ExerciseId);
        if (existing is not null)
        {
            return await MapWorkoutExerciseAsync(existing, cancellationToken);
        }

        var workoutExercise = new WorkoutExercise
        {
            Id = Guid.NewGuid(),
            WorkoutId = workout.Id,
            ExerciseId = exercise.Id,
            Exercise = exercise,
            SortOrder = workout.Exercises.Count == 0 ? 0 : workout.Exercises.Max(item => item.SortOrder) + 1
        };

        db.WorkoutExercises.Add(workoutExercise);
        await db.SaveChangesAsync(cancellationToken);

        workoutExercise.Sets = [];
        return await MapWorkoutExerciseAsync(workoutExercise, cancellationToken);
    }

    public async Task<bool> RemoveExerciseAsync(Guid workoutId, Guid workoutExerciseId, CancellationToken cancellationToken)
    {
        var workout = await LoadWorkoutAsync(workoutId, cancellationToken);
        if (workout is null || !workout.IsActive)
        {
            return false;
        }

        var exercise = workout.Exercises.FirstOrDefault(item => item.Id == workoutExerciseId);
        if (exercise is null)
        {
            return false;
        }

        db.WorkoutExercises.Remove(exercise);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AddSetResponse?> AddSetAsync(
        Guid workoutId,
        Guid workoutExerciseId,
        AddSetRequest request,
        CancellationToken cancellationToken)
    {
        ValidateSet(request.WeightKg, request.Reps);

        var workout = await LoadWorkoutAsync(workoutId, cancellationToken);
        if (workout is null || !workout.IsActive)
        {
            return null;
        }

        var workoutExercise = workout.Exercises.FirstOrDefault(item => item.Id == workoutExerciseId);
        if (workoutExercise is null)
        {
            return null;
        }

        var set = new WorkoutSet
        {
            Id = Guid.NewGuid(),
            WorkoutExerciseId = workoutExercise.Id,
            SetNumber = workoutExercise.Sets.Count == 0 ? 1 : workoutExercise.Sets.Max(item => item.SetNumber) + 1,
            WeightKg = decimal.Round(request.WeightKg, 2),
            Reps = request.Reps,
            CompletedAt = DateTimeOffset.UtcNow
        };

        db.WorkoutSets.Add(set);
        await db.SaveChangesAsync(cancellationToken);

        if (workoutExercise.Sets.All(item => item.Id != set.Id))
        {
            workoutExercise.Sets.Add(set);
        }

        var records = await PersonalRecordCalculator.DetectHitsAsync(db, currentUser.UserId, workoutExercise, set, cancellationToken);
        return new AddSetResponse(
            MapSet(set),
            await MapWorkoutExerciseAsync(workoutExercise, cancellationToken),
            records);
    }

    public async Task<WorkoutExerciseDto?> UpdateSetAsync(
        Guid workoutId,
        Guid workoutExerciseId,
        Guid setId,
        UpdateSetRequest request,
        CancellationToken cancellationToken)
    {
        ValidateSet(request.WeightKg, request.Reps);

        var workout = await LoadWorkoutAsync(workoutId, cancellationToken);
        if (workout is null || !workout.IsActive)
        {
            return null;
        }

        var workoutExercise = workout.Exercises.FirstOrDefault(item => item.Id == workoutExerciseId);
        var set = workoutExercise?.Sets.FirstOrDefault(item => item.Id == setId);
        if (workoutExercise is null || set is null)
        {
            return null;
        }

        set.WeightKg = decimal.Round(request.WeightKg, 2);
        set.Reps = request.Reps;
        await db.SaveChangesAsync(cancellationToken);

        return await MapWorkoutExerciseAsync(workoutExercise, cancellationToken);
    }

    public async Task<bool> DeleteSetAsync(
        Guid workoutId,
        Guid workoutExerciseId,
        Guid setId,
        CancellationToken cancellationToken)
    {
        var workout = await LoadWorkoutAsync(workoutId, cancellationToken);
        if (workout is null || !workout.IsActive)
        {
            return false;
        }

        var workoutExercise = workout.Exercises.FirstOrDefault(item => item.Id == workoutExerciseId);
        var set = workoutExercise?.Sets.FirstOrDefault(item => item.Id == setId);
        if (workoutExercise is null || set is null)
        {
            return false;
        }

        db.WorkoutSets.Remove(set);
        await db.SaveChangesAsync(cancellationToken);

        var remaining = workoutExercise.Sets
            .Where(item => item.Id != setId)
            .OrderBy(item => item.SetNumber)
            .ToList();

        for (var index = 0; index < remaining.Count; index++)
        {
            remaining[index].SetNumber = index + 1;
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<Workout?> LoadWorkoutAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db.Workouts
            .Where(workout => workout.Id == id && workout.UserId == currentUser.UserId)
            .Include(workout => workout.Exercises)
                .ThenInclude(exercise => exercise.Sets)
            .Include(workout => workout.Exercises)
                .ThenInclude(exercise => exercise.Exercise)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<WorkoutDto> MapDetailAsync(Workout workout, CancellationToken cancellationToken)
    {
        var exercises = new List<WorkoutExerciseDto>();
        foreach (var exercise in workout.Exercises.OrderBy(item => item.SortOrder))
        {
            exercises.Add(await MapWorkoutExerciseAsync(exercise, cancellationToken));
        }

        return new WorkoutDto(
            workout.Id,
            workout.StartedAt,
            workout.FinishedAt,
            workout.IsActive,
            workout.Notes,
            exercises,
            exercises.SelectMany(item => item.Sets).Sum(set => set.VolumeKg));
    }

    private async Task<WorkoutExerciseDto> MapWorkoutExerciseAsync(
        WorkoutExercise workoutExercise,
        CancellationToken cancellationToken)
    {
        var lastSession = await GetLastSessionAsync(workoutExercise.ExerciseId, workoutExercise.WorkoutId, cancellationToken);

        return new WorkoutExerciseDto(
            workoutExercise.Id,
            workoutExercise.ExerciseId,
            workoutExercise.Exercise.Name,
            workoutExercise.Exercise.MuscleGroups,
            workoutExercise.Exercise.Equipment.ToString(),
            workoutExercise.SortOrder,
            workoutExercise.Sets.OrderBy(set => set.SetNumber).Select(MapSet).ToList(),
            lastSession);
    }

    private async Task<LastSessionDto?> GetLastSessionAsync(
        Guid exerciseId,
        Guid currentWorkoutId,
        CancellationToken cancellationToken)
    {
        var previous = await db.WorkoutExercises
            .AsNoTracking()
            .Where(item =>
                item.ExerciseId == exerciseId &&
                item.Workout.UserId == currentUser.UserId &&
                item.WorkoutId != currentWorkoutId &&
                item.Workout.FinishedAt != null &&
                item.Sets.Any())
            .OrderByDescending(item => item.Workout.StartedAt)
            .Select(item => new
            {
                item.Workout.StartedAt,
                Sets = item.Sets
                    .OrderBy(set => set.SetNumber)
                    .Select(set => new
                    {
                        set.Id,
                        set.SetNumber,
                        set.WeightKg,
                        set.Reps,
                        set.CompletedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (previous is null)
        {
            return null;
        }

        return new LastSessionDto(
            previous.StartedAt,
            previous.Sets.Select(set => new SetDto(
                set.Id,
                set.SetNumber,
                set.WeightKg,
                set.Reps,
                StrengthMetrics.Volume(set.WeightKg, set.Reps),
                StrengthMetrics.EstimatedOneRepMax(set.WeightKg, set.Reps),
                set.CompletedAt)).ToList());
    }

    private static WorkoutSummaryDto MapSummary(Workout workout)
    {
        var sets = workout.Exercises.SelectMany(exercise => exercise.Sets).ToList();
        return new WorkoutSummaryDto(
            workout.Id,
            workout.StartedAt,
            workout.FinishedAt,
            workout.IsActive,
            workout.Exercises.Count,
            sets.Count,
            sets.Sum(set => set.Volume),
            workout.Exercises
                .OrderBy(exercise => exercise.SortOrder)
                .Select(exercise => exercise.Exercise.Name)
                .ToList());
    }

    private static SetDto MapSet(WorkoutSet set) => new(
        set.Id,
        set.SetNumber,
        set.WeightKg,
        set.Reps,
        set.Volume,
        set.EstimatedOneRepMax,
        set.CompletedAt);

    private static void ValidateSet(decimal weightKg, int reps)
    {
        if (weightKg <= 0 || weightKg > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(weightKg), "Vikten måste vara mellan 0 och 1000 kg.");
        }

        if (reps is <= 0 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(reps), "Reps måste vara mellan 1 och 200.");
        }
    }
}
