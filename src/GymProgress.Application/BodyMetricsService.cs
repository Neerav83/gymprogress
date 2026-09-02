using GymProgress.Application.Contracts;
using GymProgress.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymProgress.Application;

public sealed class BodyMetricsService(IApplicationDbContext db)
{
    public async Task<BodyMetricsHistoryDto> GetMetricsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var metrics = await db.BodyMetrics
            .AsNoTracking()
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.Date)
            .Select(m => new BodyMetricsDto(
                m.Id,
                m.Date,
                m.WeightKg,
                m.HeightCm,
                m.ChestCm,
                m.WaistCm,
                m.HipsCm,
                m.ArmCm,
                m.ThighCm,
                m.Notes))
            .ToListAsync(cancellationToken);

        return new BodyMetricsHistoryDto(metrics);
    }

    public async Task<BodyMetricsDto> AddMetricsAsync(
        Guid userId,
        AddBodyMetricsRequest request,
        CancellationToken cancellationToken)
    {
        var metrics = new BodyMetrics
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = DateTimeOffset.UtcNow,
            WeightKg = request.WeightKg,
            HeightCm = request.HeightCm,
            ChestCm = request.ChestCm,
            WaistCm = request.WaistCm,
            HipsCm = request.HipsCm,
            ArmCm = request.ArmCm,
            ThighCm = request.ThighCm,
            Notes = request.Notes
        };

        db.BodyMetrics.Add(metrics);
        await db.SaveChangesAsync(cancellationToken);

        return new BodyMetricsDto(
            metrics.Id,
            metrics.Date,
            metrics.WeightKg,
            metrics.HeightCm,
            metrics.ChestCm,
            metrics.WaistCm,
            metrics.HipsCm,
            metrics.ArmCm,
            metrics.ThighCm,
            metrics.Notes);
    }

    public async Task<BodyMetricsDto?> UpdateMetricsAsync(
        Guid userId,
        Guid metricsId,
        UpdateBodyMetricsRequest request,
        CancellationToken cancellationToken)
    {
        var metrics = await db.BodyMetrics
            .FirstOrDefaultAsync(m => m.Id == metricsId && m.UserId == userId, cancellationToken);

        if (metrics is null)
        {
            return null;
        }

        if (request.WeightKg.HasValue) metrics.WeightKg = request.WeightKg;
        if (request.HeightCm.HasValue) metrics.HeightCm = request.HeightCm;
        if (request.ChestCm.HasValue) metrics.ChestCm = request.ChestCm;
        if (request.WaistCm.HasValue) metrics.WaistCm = request.WaistCm;
        if (request.HipsCm.HasValue) metrics.HipsCm = request.HipsCm;
        if (request.ArmCm.HasValue) metrics.ArmCm = request.ArmCm;
        if (request.ThighCm.HasValue) metrics.ThighCm = request.ThighCm;
        if (request.Notes is not null) metrics.Notes = request.Notes;

        await db.SaveChangesAsync(cancellationToken);

        return new BodyMetricsDto(
            metrics.Id,
            metrics.Date,
            metrics.WeightKg,
            metrics.HeightCm,
            metrics.ChestCm,
            metrics.WaistCm,
            metrics.HipsCm,
            metrics.ArmCm,
            metrics.ThighCm,
            metrics.Notes);
    }

    public async Task<bool> DeleteMetricsAsync(
        Guid userId,
        Guid metricsId,
        CancellationToken cancellationToken)
    {
        var metrics = await db.BodyMetrics
            .FirstOrDefaultAsync(m => m.Id == metricsId && m.UserId == userId, cancellationToken);

        if (metrics is null)
        {
            return false;
        }

        db.BodyMetrics.Remove(metrics);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
