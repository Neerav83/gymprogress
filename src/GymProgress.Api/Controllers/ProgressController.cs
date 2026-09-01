using GymProgress.Application;
using GymProgress.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GymProgress.Api.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class ProgressController(ProgressService progress) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> Dashboard(CancellationToken cancellationToken)
    {
        return Ok(await progress.GetDashboardAsync(cancellationToken));
    }

    [HttpGet("progress/{exerciseId:guid}")]
    public async Task<ActionResult<ExerciseProgressDto>> Progress(
        Guid exerciseId,
        [FromQuery] string range = "all",
        CancellationToken cancellationToken = default)
    {
        var result = await progress.GetAsync(exerciseId, range, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("personal-records")]
    public async Task<ActionResult<IReadOnlyList<PersonalRecordDto>>> PersonalRecords(
        [FromQuery] Guid? exerciseId,
        CancellationToken cancellationToken)
    {
        return Ok(await progress.ListRecordsAsync(exerciseId, cancellationToken));
    }
}
