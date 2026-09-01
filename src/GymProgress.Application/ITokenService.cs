namespace GymProgress.Application;

public interface ITokenService
{
    string CreateAccessToken(Guid userId, string email, string displayName);
    string GenerateRefreshToken();
}
