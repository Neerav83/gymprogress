using GymProgress.Application;
using GymProgress.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GymProgress.Api.Controllers;

[ApiController]
[Route("api/v1/exercises")]
public sealed class ExercisesController(ExerciseService exercises) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExerciseDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await exercises.ListAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExerciseDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var exercise = await exercises.GetAsync(id, cancellationToken);
        return exercise is null ? NotFound() : Ok(exercise);
    }
}
