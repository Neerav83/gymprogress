using GymProgress.Application;
using GymProgress.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GymProgress.Api.Controllers;

[ApiController]
[Route("api/v1/workouts")]
public sealed class WorkoutsController(WorkoutService workouts) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WorkoutSummaryDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await workouts.ListAsync(cancellationToken));
    }

    [HttpGet("active")]
    public async Task<ActionResult<WorkoutDto>> Active(CancellationToken cancellationToken)
    {
        var workout = await workouts.GetActiveAsync(cancellationToken);
        return workout is null ? NoContent() : Ok(workout);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkoutDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var workout = await workouts.GetAsync(id, cancellationToken);
        return workout is null ? NotFound() : Ok(workout);
    }

    [HttpPost]
    public async Task<ActionResult<WorkoutDto>> Create(
        [FromBody] CreateWorkoutRequest? request,
        CancellationToken cancellationToken)
    {
        var workout = await workouts.CreateAsync(request ?? new CreateWorkoutRequest(null), cancellationToken);
        return Ok(workout);
    }

    [HttpPost("from-recommendation")]
    public async Task<ActionResult<WorkoutDto>> CreateFromRecommendation(
        [FromBody] CreateWorkoutFromRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var workout = await workouts.CreateFromRecommendationAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = workout.Id }, workout);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPost("from-template/{templateId:guid}")]
    public async Task<ActionResult<WorkoutDto>> CreateFromTemplate(
        Guid templateId,
        CancellationToken cancellationToken)
    {
        try
        {
            var workout = await workouts.CreateFromTemplateAsync(templateId, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = workout.Id }, workout);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPost("{id:guid}/finish")]
    public async Task<ActionResult<WorkoutDto>> Finish(Guid id, CancellationToken cancellationToken)
    {
        var workout = await workouts.FinishAsync(id, cancellationToken);
        return workout is null ? NotFound() : Ok(workout);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await workouts.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/exercises")]
    public async Task<ActionResult<WorkoutExerciseDto>> AddExercise(
        Guid id,
        [FromBody] AddExerciseRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var exercise = await workouts.AddExerciseAsync(id, request, cancellationToken);
            return exercise is null ? NotFound() : Ok(exercise);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpDelete("{id:guid}/exercises/{workoutExerciseId:guid}")]
    public async Task<IActionResult> RemoveExercise(
        Guid id,
        Guid workoutExerciseId,
        CancellationToken cancellationToken)
    {
        return await workouts.RemoveExerciseAsync(id, workoutExerciseId, cancellationToken)
            ? NoContent()
            : NotFound();
    }

    [HttpPost("{id:guid}/exercises/{workoutExerciseId:guid}/sets")]
    public async Task<ActionResult<AddSetResponse>> AddSet(
        Guid id,
        Guid workoutExerciseId,
        [FromBody] AddSetRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await workouts.AddSetAsync(id, workoutExerciseId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPut("{id:guid}/exercises/{workoutExerciseId:guid}/sets/{setId:guid}")]
    public async Task<ActionResult<WorkoutExerciseDto>> UpdateSet(
        Guid id,
        Guid workoutExerciseId,
        Guid setId,
        [FromBody] UpdateSetRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var exercise = await workouts.UpdateSetAsync(id, workoutExerciseId, setId, request, cancellationToken);
            return exercise is null ? NotFound() : Ok(exercise);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpDelete("{id:guid}/exercises/{workoutExerciseId:guid}/sets/{setId:guid}")]
    public async Task<IActionResult> DeleteSet(
        Guid id,
        Guid workoutExerciseId,
        Guid setId,
        CancellationToken cancellationToken)
    {
        return await workouts.DeleteSetAsync(id, workoutExerciseId, setId, cancellationToken)
            ? NoContent()
            : NotFound();
    }
}
