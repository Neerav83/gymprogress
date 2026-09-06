namespace GymProgress.Application;

public interface IAuditLogger
{
    void LogLoginSuccess(Guid userId, string email, string? ipAddress);
    void LogLoginFailure(string email, string? ipAddress, string reason);
    void LogRegistration(Guid userId, string email, string? ipAddress);
    void LogPasswordChange(Guid userId, string? ipAddress);
    void LogTokenRefresh(Guid userId, string? ipAddress);
    void LogProfileUpdate(Guid userId, string? ipAddress);
    void LogLogout(Guid userId, string? ipAddress);
}
