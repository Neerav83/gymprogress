using GymProgress.Application;
using GymProgress.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GymProgress.Api.Controllers;

[ApiController]
[Route("api/v1/body-metrics")]
public sealed class BodyMetricsController(
    BodyMetricsService bodyMetricsService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<BodyMetricsHistoryDto>> GetMetrics(CancellationToken cancellationToken)
    {
        return Ok(await bodyMetricsService.GetMetricsAsync(currentUser.UserId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<BodyMetricsDto>> AddMetrics(
        [FromBody] AddBodyMetricsRequest request,
        CancellationToken cancellationToken)
    {
        var metrics = await bodyMetricsService.AddMetricsAsync(
            currentUser.UserId,
            request,
            cancellationToken);

        return Created($"/api/v1/body-metrics/{metrics.Id}", metrics);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BodyMetricsDto>> UpdateMetrics(
        Guid id,
        [FromBody] UpdateBodyMetricsRequest request,
        CancellationToken cancellationToken)
    {
        var metrics = await bodyMetricsService.UpdateMetricsAsync(
            currentUser.UserId,
            id,
            request,
            cancellationToken);

        return metrics is null ? NotFound() : Ok(metrics);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMetrics(Guid id, CancellationToken cancellationToken)
    {
        var success = await bodyMetricsService.DeleteMetricsAsync(
            currentUser.UserId,
            id,
            cancellationToken);

        return success ? NoContent() : NotFound();
    }
}
