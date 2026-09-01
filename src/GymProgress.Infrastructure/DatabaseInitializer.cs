using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GymProgress.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        await db.Database.MigrateAsync(cancellationToken);
        await SeedAsync(db, logger, cancellationToken);
    }

    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken cancellationToken)
    {
        if (!await db.Users.AnyAsync(user => user.Id == CatalogSeed.DefaultUser().Id, cancellationToken))
        {
            db.Users.Add(CatalogSeed.DefaultUser());
            logger.LogInformation("Seedade standardanvändare.");
        }

        var existingNames = await db.Exercises.Select(exercise => exercise.Name).ToListAsync(cancellationToken);
        var missing = CatalogSeed.Exercises()
            .Where(exercise => !existingNames.Contains(exercise.Name))
            .ToList();

        if (missing.Count > 0)
        {
            db.Exercises.AddRange(missing);
            logger.LogInformation("Seedade {Count} övningar.", missing.Count);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
