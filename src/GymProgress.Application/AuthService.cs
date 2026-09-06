using GymProgress.Application.Contracts;
using GymProgress.Domain;
using GymProgress.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GymProgress.Application;

public sealed class AuthService(
    IApplicationDbContext db, 
    ITokenService tokens, 
    IOptions<JwtOptions> jwtOptions,
    IAuditLogger auditLogger,
    IClientInfo clientInfo)
{
    private readonly PasswordHasher<User> _passwords = new();

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var displayName = RequireDisplayName(request.DisplayName);
        ValidatePassword(request.Password);

        if (await db.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            auditLogger.LogLoginFailure(email, clientInfo.GetIpAddress(), "Email redan registrerad");
            throw new InvalidOperationException("Ett konto med denna e-postadress kunde inte skapas.");
        }

        var user = await ClaimSeedUserOrCreateAsync(email, displayName, cancellationToken);
        user.PasswordHash = _passwords.HashPassword(user, request.Password);
        await db.SaveChangesAsync(cancellationToken);

        auditLogger.LogRegistration(user.Id, email, clientInfo.GetIpAddress());

        return await ToResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await db.Users.FirstOrDefaultAsync(item => item.Email == email, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            auditLogger.LogLoginFailure(email, clientInfo.GetIpAddress(), "Användare hittades inte");
            return null;
        }

        var result = _passwords.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            auditLogger.LogLoginFailure(email, clientInfo.GetIpAddress(), "Fel lösenord");
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwords.HashPassword(user, request.Password);
            await db.SaveChangesAsync(cancellationToken);
        }

        auditLogger.LogLoginSuccess(user.Id, email, clientInfo.GetIpAddress());

        return await ToResponseAsync(user, cancellationToken);
    }

    public async Task<UserDto?> GetMeAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(user => user.Id == userId && user.Email != null)
            .Select(user => new UserDto(user.Id, user.Email!, user.DisplayName, user.ProfileImageUrl, user.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<User> ClaimSeedUserOrCreateAsync(string email, string displayName, CancellationToken cancellationToken)
    {
        var claimable = await db.Users.FirstOrDefaultAsync(
            user => user.Id == KnownIds.DefaultUserId && user.Email == null && user.PasswordHash == null,
            cancellationToken);

        var hasRegisteredUsers = await db.Users.AnyAsync(user => user.Email != null, cancellationToken);
        if (claimable is not null && !hasRegisteredUsers)
        {
            claimable.Email = email;
            claimable.DisplayName = displayName;
            return claimable;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = displayName,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Users.Add(user);
        return user;
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshTokenValue, CancellationToken cancellationToken)
    {
        var refreshToken = await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenValue, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            return null;
        }

        if (refreshToken.User.Email is null)
        {
            return null;
        }

        refreshToken.RevokedAt = DateTimeOffset.UtcNow;

        var activeTokenCount = await db.RefreshTokens
            .CountAsync(rt => rt.UserId == refreshToken.UserId && rt.IsActive, cancellationToken);

        if (activeTokenCount >= 5)
        {
            var oldestTokens = await db.RefreshTokens
                .Where(rt => rt.UserId == refreshToken.UserId && rt.IsActive)
                .OrderBy(rt => rt.CreatedAt)
                .Take(activeTokenCount - 4)
                .ToListAsync(cancellationToken);

            foreach (var token in oldestTokens)
            {
                token.RevokedAt = DateTimeOffset.UtcNow;
            }
        }

        var newRefreshToken = CreateRefreshToken(refreshToken.UserId);
        db.RefreshTokens.Add(newRefreshToken);
        await db.SaveChangesAsync(cancellationToken);

        auditLogger.LogTokenRefresh(refreshToken.UserId, clientInfo.GetIpAddress());

        return ToResponse(refreshToken.User, newRefreshToken.Token);
    }

    public async Task RevokeAllTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var activeTokens = await db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.IsActive)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var token in activeTokens)
        {
            token.RevokedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);

        auditLogger.LogLogout(userId, clientInfo.GetIpAddress());
    }

    private RefreshToken CreateRefreshToken(Guid userId)
    {
        var expirationDays = Math.Clamp(jwtOptions.Value.RefreshTokenExpirationDays, 1, 365);
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = tokens.GenerateRefreshToken(),
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(expirationDays)
        };
    }

    private async Task<AuthResponse> ToResponseAsync(User user, CancellationToken cancellationToken)
    {
        var refreshToken = CreateRefreshToken(user.Id);
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(user, refreshToken.Token);
    }

    private AuthResponse ToResponse(User user, string refreshToken) => new(
        tokens.CreateAccessToken(user.Id, user.Email!, user.DisplayName),
        refreshToken,
        new UserDto(user.Id, user.Email!, user.DisplayName, user.ProfileImageUrl, user.CreatedAt));

    private static string NormalizeEmail(string? email)
    {
        var value = email?.Trim().ToLowerInvariant() ?? "";
        if (value.Length is < 5 or > 320 || !value.Contains('@'))
        {
            throw new ArgumentException("Ange en giltig e-postadress.");
        }

        return value;
    }

    private static string RequireDisplayName(string? displayName)
    {
        var value = displayName?.Trim() ?? "";
        if (value.Length is < 2 or > 100)
        {
            throw new ArgumentException("Namnet måste vara 2–100 tecken.");
        }

        return value;
    }

    private static void ValidatePassword(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
        {
            throw new ArgumentException("Lösenordet måste vara minst 8 tecken.");
        }
    }
}
