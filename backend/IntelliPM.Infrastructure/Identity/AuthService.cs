using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Entities;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(AppDbContext context, PasswordHasher passwordHasher, ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> LoginAsync(string username, string password, CancellationToken ct)
    {
        // Allow login with either username or email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username || u.Email == username, ct);
        
        if (user == null)
            throw new UnauthorizedException($"User with username or email '{username}' not found");
        
        if (!user.IsActive)
            throw new UnauthorizedException("User account is inactive");
        
        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            throw new UnauthorizedException("Invalid password");

        // Update last login timestamp
        user.LastLoginAt = DateTimeOffset.UtcNow;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        // Include GlobalRole in JWT token roles for authorization checks
        var roles = new List<string> { user.GlobalRole.ToString() };
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Username, user.Email, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token
        var rt = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };
        _context.RefreshTokens.Add(rt);
        await _context.SaveChangesAsync(ct);

        return new LoginResult(
            user.Id,
            user.Username,
            user.Email,
            user.GlobalRole.ToString(),
            accessToken,
            refreshToken
        );
    }

    public async Task<int> RegisterAsync(string username, string email, string password, string firstName, string lastName, CancellationToken ct)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username, ct))
            throw new Application.Common.Exceptions.ApplicationException("Username already exists");

        if (await _context.Users.AnyAsync(u => u.Email == email, ct))
            throw new Application.Common.Exceptions.ApplicationException("Email already exists");

        // For public registration, get or create a default organization
        // In a multi-tenant system, new users need an organization
        var defaultOrganization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Code == "default", ct);
        if (defaultOrganization == null)
        {
            // Create a default organization if none exists
            defaultOrganization = new Organization
            {
                Name = "Default Organization",
                Code = "default",
                CreatedAt = DateTimeOffset.UtcNow
            };
            _context.Organizations.Add(defaultOrganization);
            await _context.SaveChangesAsync(ct);
        }

        var (hash, salt) = _passwordHasher.HashPassword(password);

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            FirstName = firstName,
            LastName = lastName,
            OrganizationId = defaultOrganization.Id,
            GlobalRole = Domain.Enums.GlobalRole.User,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        return user.Id;
    }

    public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken, CancellationToken ct)
    {
        var rt = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken && !r.IsRevoked && r.ExpiresAt > DateTimeOffset.UtcNow, ct);
        
        if (rt == null)
            throw new UnauthorizedException("Invalid refresh token");

        var user = await _context.Users.FindAsync(new object[] { rt.UserId }, cancellationToken: ct);
        if (user == null)
            throw new UnauthorizedException("User not found");

        // Include GlobalRole in JWT token roles for authorization checks
        var roles = new List<string> { user.GlobalRole.ToString() };

        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Username, user.Email, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        rt.IsRevoked = true;
        var newRt = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(newRt);
        await _context.SaveChangesAsync(ct);

        return (newAccessToken, newRefreshToken);
    }

    public async System.Threading.Tasks.Task RevokeTokenAsync(int userId, string refreshToken, CancellationToken ct)
    {
        var rt = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken && r.UserId == userId, ct);
        
        if (rt != null)
        {
            rt.IsRevoked = true;
            await _context.SaveChangesAsync(ct);
        }
    }
}

