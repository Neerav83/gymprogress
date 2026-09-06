using GymProgress.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GymProgress.Infrastructure;

public sealed class RefreshTokenCleanupService(
    IServiceProvider serviceProvider,
    ILogger<RefreshTokenCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Refresh token cleanup service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                await CleanupExpiredTokensAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error during refresh token cleanup.");
            }
        }

        logger.LogInformation("Refresh token cleanup service stopped.");
    }

    private async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var cutoffDate = DateTimeOffset.UtcNow.AddDays(-7);
        
        var expiredTokens = await db.RefreshTokens
            .Where(token => token.ExpiresAt < cutoffDate || token.RevokedAt < cutoffDate)
            .ToListAsync(cancellationToken);

        if (expiredTokens.Count > 0)
        {
            db.RefreshTokens.RemoveRange(expiredTokens);
            await db.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation(
                "Cleaned up {Count} expired refresh tokens.",
                expiredTokens.Count);
        }
    }
}
