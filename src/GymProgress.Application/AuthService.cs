using GymProgress.Application.Contracts;
using GymProgress.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GymProgress.Application;

public sealed class AuthService(IApplicationDbContext db, ITokenService tokens)
{
    private readonly PasswordHasher<User> _passwords = new();

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var displayName = RequireDisplayName(request.DisplayName);
        ValidatePassword(request.Password);

        if (await db.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            throw new InvalidOperationException("E-postadressen är redan registrerad.");
        }

        var user = await ClaimSeedUserOrCreateAsync(email, displayName, cancellationToken);
        user.PasswordHash = _passwords.HashPassword(user, request.Password);
        await db.SaveChangesAsync(cancellationToken);

        return ToResponse(user);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await db.Users.FirstOrDefaultAsync(item => item.Email == email, cancellationToken);
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return null;
        }

        var result = _passwords.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwords.HashPassword(user, request.Password);
            await db.SaveChangesAsync(cancellationToken);
        }

        return ToResponse(user);
    }

    public async Task<UserDto?> GetMeAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(user => user.Id == userId && user.Email != null)
            .Select(user => new UserDto(user.Id, user.Email!, user.DisplayName))
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

    private AuthResponse ToResponse(User user) => new(
        tokens.Create(user.Id, user.Email!, user.DisplayName),
        new UserDto(user.Id, user.Email!, user.DisplayName));

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
