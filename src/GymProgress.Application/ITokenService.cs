namespace GymProgress.Application;

public interface ITokenService
{
    string Create(Guid userId, string email, string displayName);
}
