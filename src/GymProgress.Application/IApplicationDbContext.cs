using GymProgress.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymProgress.Application;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<Workout> Workouts { get; }
    DbSet<WorkoutExercise> WorkoutExercises { get; }
    DbSet<WorkoutSet> WorkoutSets { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
