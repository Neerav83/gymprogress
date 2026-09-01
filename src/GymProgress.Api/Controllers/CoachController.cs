using GymProgress.Application;
using GymProgress.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GymProgress.Api.Controllers;

[ApiController]
[Route("api/v1/coach")]
public sealed class CoachController(CoachService coach) : ControllerBase
{
    [HttpGet("recommendation")]
    public async Task<ActionResult<WorkoutRecommendationDto>> Recommendation(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await coach.GetTodaysRecommendationAsync(cancellationToken));
        }
        catch (CoachUnavailableException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = exception.Message });
        }
        catch (CoachInvalidResponseException exception)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = exception.Message });
        }
    }
}
