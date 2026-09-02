using GymProgress.Application;
using GymProgress.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace GymProgress.Api.Controllers;

[ApiController]
[Route("api/v1/profile")]
public sealed class ProfileController(
    ProfileService profileService,
    ICurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserProfileDto>> GetProfile(CancellationToken cancellationToken)
    {
        var profile = await profileService.GetProfileAsync(currentUser.UserId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var profile = await profileService.UpdateProfileAsync(
            currentUser.UserId,
            request,
            cancellationToken);

        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var success = await profileService.ChangePasswordAsync(
                currentUser.UserId,
                request,
                cancellationToken);

            return success
                ? Ok()
                : Unauthorized(new { error = "Fel nuvarande lösenord." });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }
}
