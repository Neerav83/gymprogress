using GymProgress.Application.Contracts;
using GymProgress.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymProgress.Application;

public sealed class ProfileService(IApplicationDbContext db, IAuditLogger auditLogger, IClientInfo clientInfo)
{
    private readonly PasswordHasher<User> _passwords = new();

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(user => user.Id == userId && user.Email != null)
            .Select(user => new UserProfileDto(
                user.Id,
                user.Email!,
                user.DisplayName,
                user.ProfileImageUrl,
                user.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserProfileDto?> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            user.DisplayName = request.DisplayName.Trim();
        }

        if (request.ProfileImageUrl is not null)
        {
            var imageUrl = string.IsNullOrWhiteSpace(request.ProfileImageUrl)
                ? null
                : request.ProfileImageUrl.Trim();

            if (imageUrl is not null && !UrlValidator.IsValidAndSafeUrl(imageUrl))
            {
                throw new ArgumentException("Profil-URL:en är ogiltig eller blockerad av säkerhetsskäl.");
            }

            user.ProfileImageUrl = imageUrl;
        }

        await db.SaveChangesAsync(cancellationToken);

        auditLogger.LogProfileUpdate(userId, clientInfo.GetIpAddress());

        return new UserProfileDto(
            user.Id,
            user.Email!,
            user.DisplayName,
            user.ProfileImageUrl,
            user.CreatedAt);
    }

    public async Task<bool> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return false;
        }

        var result = _passwords.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (result == PasswordVerificationResult.Failed)
        {
            return false;
        }

        if (request.NewPassword.Length < 8)
        {
            throw new ArgumentException("Lösenordet måste vara minst 8 tecken.");
        }

        user.PasswordHash = _passwords.HashPassword(user, request.NewPassword);
        await db.SaveChangesAsync(cancellationToken);

        auditLogger.LogPasswordChange(userId, clientInfo.GetIpAddress());

        var now = DateTimeOffset.UtcNow;
        var activeTokens = await db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > now)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
