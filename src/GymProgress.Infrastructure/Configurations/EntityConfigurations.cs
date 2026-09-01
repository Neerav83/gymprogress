using GymProgress.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GymProgress.Infrastructure.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.Email).HasMaxLength(320);
        builder.Property(user => user.PasswordHash).HasMaxLength(500);
        builder.HasIndex(user => user.Email).IsUnique().HasFilter("\"Email\" IS NOT NULL");
        builder.Property(user => user.CreatedAt).IsRequired();
    }
}

public sealed class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("exercises");
        builder.HasKey(exercise => exercise.Id);
        builder.Property(exercise => exercise.Name).HasMaxLength(120).IsRequired();
        builder.HasIndex(exercise => exercise.Name).IsUnique();
        builder.Property(exercise => exercise.MuscleGroups).HasColumnType("text[]");
        builder.Property(exercise => exercise.Equipment).HasConversion<string>().HasMaxLength(32);
    }
}

public sealed class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.ToTable("workouts");
        builder.HasKey(workout => workout.Id);
        builder.Property(workout => workout.Notes).HasMaxLength(500);
        builder.HasIndex(workout => new { workout.UserId, workout.StartedAt });
        builder.HasOne(workout => workout.User)
            .WithMany(user => user.Workouts)
            .HasForeignKey(workout => workout.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutExercise> builder)
    {
        builder.ToTable("workout_exercises");
        builder.HasKey(exercise => exercise.Id);
        builder.HasIndex(exercise => new { exercise.WorkoutId, exercise.ExerciseId }).IsUnique();
        builder.HasOne(exercise => exercise.Workout)
            .WithMany(workout => workout.Exercises)
            .HasForeignKey(exercise => exercise.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(exercise => exercise.Exercise)
            .WithMany(item => item.WorkoutExercises)
            .HasForeignKey(exercise => exercise.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class WorkoutSetConfiguration : IEntityTypeConfiguration<WorkoutSet>
{
    public void Configure(EntityTypeBuilder<WorkoutSet> builder)
    {
        builder.ToTable("workout_sets");
        builder.HasKey(set => set.Id);
        builder.Property(set => set.WeightKg).HasPrecision(6, 2);
        builder.Ignore(set => set.Volume);
        builder.Ignore(set => set.EstimatedOneRepMax);
        builder.HasOne(set => set.WorkoutExercise)
            .WithMany(exercise => exercise.Sets)
            .HasForeignKey(set => set.WorkoutExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(token => token.Id);
        builder.Property(token => token.Token).HasMaxLength(128).IsRequired();
        builder.HasIndex(token => token.Token).IsUnique();
        builder.HasIndex(token => new { token.UserId, token.ExpiresAt });
        builder.Ignore(token => token.IsActive);
        builder.HasOne(token => token.User)
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
