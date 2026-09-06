using GymProgress.Application;
using Microsoft.Extensions.Logging;

namespace GymProgress.Infrastructure;

public sealed class AuditLogger(ILogger<AuditLogger> logger) : IAuditLogger
{
    public void LogLoginSuccess(Guid userId, string email, string? ipAddress)
    {
        logger.LogInformation(
            "[AUDIT] Login SUCCESS - User: {UserId}, Email: {Email}, IP: {IpAddress}",
            userId, MaskEmail(email), MaskIpAddress(ipAddress));
    }

    public void LogLoginFailure(string email, string? ipAddress, string reason)
    {
        logger.LogWarning(
            "[AUDIT] Login FAILURE - Email: {Email}, IP: {IpAddress}, Reason: {Reason}",
            MaskEmail(email), MaskIpAddress(ipAddress), reason);
    }

    public void LogRegistration(Guid userId, string email, string? ipAddress)
    {
        logger.LogInformation(
            "[AUDIT] Registration - User: {UserId}, Email: {Email}, IP: {IpAddress}",
            userId, MaskEmail(email), MaskIpAddress(ipAddress));
    }

    public void LogPasswordChange(Guid userId, string? ipAddress)
    {
        logger.LogInformation(
            "[AUDIT] Password Changed - User: {UserId}, IP: {IpAddress}",
            userId, MaskIpAddress(ipAddress));
    }

    public void LogTokenRefresh(Guid userId, string? ipAddress)
    {
        logger.LogInformation(
            "[AUDIT] Token Refreshed - User: {UserId}, IP: {IpAddress}",
            userId, MaskIpAddress(ipAddress));
    }

    public void LogProfileUpdate(Guid userId, string? ipAddress)
    {
        logger.LogInformation(
            "[AUDIT] Profile Updated - User: {UserId}, IP: {IpAddress}",
            userId, MaskIpAddress(ipAddress));
    }

    public void LogLogout(Guid userId, string? ipAddress)
    {
        logger.LogInformation(
            "[AUDIT] Logout - User: {UserId}, IP: {IpAddress}",
            userId, MaskIpAddress(ipAddress));
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return "***";
        }

        var parts = email.Split('@');
        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length <= 2)
        {
            return $"***@{domain}";
        }

        return $"{localPart[0]}***{localPart[^1]}@{domain}";
    }

    private static string MaskIpAddress(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return "unknown";
        }

        if (ipAddress.Contains('.'))
        {
            var parts = ipAddress.Split('.');
            if (parts.Length == 4)
            {
                return $"{parts[0]}.{parts[1]}.***";
            }
        }

        if (ipAddress.Contains(':'))
        {
            var parts = ipAddress.Split(':');
            if (parts.Length >= 3)
            {
                return $"{parts[0]}:{parts[1]}:***";
            }
        }

        return "***";
    }
}
