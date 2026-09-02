using GymProgress.Application.Contracts;
using GymProgress.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymProgress.Application;

public sealed class WorkoutTemplateService(IApplicationDbContext db, ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<WorkoutTemplateDto>> ListAsync(CancellationToken cancellationToken)
    {
        var templates = await db.WorkoutTemplates
            .AsNoTracking()
            .Where(t => t.UserId == currentUser.UserId)
            .Include(t => t.Exercises)
                .ThenInclude(e => e.Exercise)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return templates.Select(MapDto).ToList();
    }

    public async Task<WorkoutTemplateDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await db.WorkoutTemplates
            .AsNoTracking()
            .Where(t => t.Id == id && t.UserId == currentUser.UserId)
            .Include(t => t.Exercises)
                .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(cancellationToken);

        return template is null ? null : MapDto(template);
    }

    public async Task<WorkoutTemplateDto> CreateFromWorkoutAsync(
        Guid workoutId,
        string name,
        string? description,
        CancellationToken cancellationToken)
    {
        var workout = await db.Workouts
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Exercise)
            .FirstOrDefaultAsync(w => w.Id == workoutId && w.UserId == currentUser.UserId, cancellationToken);

        if (workout is null)
        {
            throw new InvalidOperationException("Passet hittades inte.");
        }

        if (workout.Exercises.Count == 0)
        {
            throw new InvalidOperationException("Passet har inga övningar.");
        }

        var template = new WorkoutTemplate
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.UserId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.WorkoutTemplates.Add(template);

        foreach (var exercise in workout.Exercises.OrderBy(e => e.SortOrder))
        {
            var templateExercise = new WorkoutTemplateExercise
            {
                Id = Guid.NewGuid(),
                TemplateId = template.Id,
                ExerciseId = exercise.ExerciseId,
                Exercise = exercise.Exercise,
                SortOrder = exercise.SortOrder
            };
            db.WorkoutTemplateExercises.Add(templateExercise);
        }

        await db.SaveChangesAsync(cancellationToken);

        return MapDto(template);
    }

    public async Task<WorkoutTemplateDto?> UpdateAsync(
        Guid id,
        string name,
        string? description,
        IReadOnlyList<Guid> exerciseIds,
        CancellationToken cancellationToken)
    {
        var trimmedName = name.Trim();
        if (trimmedName.Length == 0)
        {
            throw new InvalidOperationException("Mallen måste ha ett namn.");
        }

        var orderedIds = exerciseIds
            .Where(exerciseId => exerciseId != Guid.Empty)
            .Distinct()
            .ToList();

        if (orderedIds.Count == 0)
        {
            throw new InvalidOperationException("Mallen måste ha minst en övning.");
        }

        var template = await db.WorkoutTemplates
            .Include(t => t.Exercises)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUser.UserId, cancellationToken);

        if (template is null)
        {
            return null;
        }

        var exercises = await db.Exercises
            .Where(exercise => orderedIds.Contains(exercise.Id))
            .ToListAsync(cancellationToken);

        if (exercises.Count != orderedIds.Count)
        {
            throw new InvalidOperationException("En eller flera övningar finns inte.");
        }

        template.Name = trimmedName;
        template.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        db.WorkoutTemplateExercises.RemoveRange(template.Exercises);

        var sortOrder = 0;
        foreach (var exerciseId in orderedIds)
        {
            var exercise = exercises.First(item => item.Id == exerciseId);
            db.WorkoutTemplateExercises.Add(new WorkoutTemplateExercise
            {
                Id = Guid.NewGuid(),
                TemplateId = template.Id,
                ExerciseId = exercise.Id,
                Exercise = exercise,
                SortOrder = sortOrder++
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        return await GetAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var template = await db.WorkoutTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUser.UserId, cancellationToken);

        if (template is null)
        {
            return false;
        }

        db.WorkoutTemplates.Remove(template);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static WorkoutTemplateDto MapDto(WorkoutTemplate template)
    {
        return new WorkoutTemplateDto(
            template.Id,
            template.Name,
            template.Description,
            template.CreatedAt,
            template.Exercises
                .OrderBy(e => e.SortOrder)
                .Select(e => new WorkoutTemplateExerciseDto(
                    e.ExerciseId,
                    e.Exercise.Name,
                    e.Exercise.MuscleGroups,
                    e.Exercise.Equipment.ToString(),
                    e.SortOrder,
                    e.SuggestedSets,
                    e.SuggestedWeight,
                    e.SuggestedRepsMin,
                    e.SuggestedRepsMax))
                .ToList());
    }
}
