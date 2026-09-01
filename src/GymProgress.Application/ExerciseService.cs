using GymProgress.Application.Contracts;
using GymProgress.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymProgress.Application;

public sealed class ExerciseService(IApplicationDbContext db)
{
    public async Task<IReadOnlyList<ExerciseDto>> ListAsync(CancellationToken cancellationToken)
    {
        return await db.Exercises
            .AsNoTracking()
            .OrderBy(exercise => exercise.Name)
            .Select(exercise => new ExerciseDto(
                exercise.Id,
                exercise.Name,
                exercise.MuscleGroups,
                exercise.Equipment.ToString()))
            .ToListAsync(cancellationToken);
    }

    public async Task<ExerciseDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return await db.Exercises
            .AsNoTracking()
            .Where(exercise => exercise.Id == id)
            .Select(exercise => new ExerciseDto(
                exercise.Id,
                exercise.Name,
                exercise.MuscleGroups,
                exercise.Equipment.ToString()))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
