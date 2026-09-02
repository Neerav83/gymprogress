using GymProgress.Application;
using GymProgress.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GymProgress.Api.Controllers;

[ApiController]
[Route("api/v1/workout-templates")]
public sealed class WorkoutTemplatesController(WorkoutTemplateService templates) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkoutTemplateDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await templates.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkoutTemplateDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var template = await templates.GetAsync(id, cancellationToken);
        return template is null ? NotFound() : Ok(template);
    }

    [HttpPost]
    public async Task<ActionResult<WorkoutTemplateDto>> Create(
        [FromBody] CreateTemplateFromWorkoutRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var template = await templates.CreateFromWorkoutAsync(
                request.WorkoutId,
                request.Name,
                request.Description,
                cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = template.Id }, template);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkoutTemplateDto>> Update(
        Guid id,
        [FromBody] UpdateWorkoutTemplateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var template = await templates.UpdateAsync(
                id,
                request.Name,
                request.Description,
                request.ExerciseIds,
                cancellationToken);
            return template is null ? NotFound() : Ok(template);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await templates.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }
}
