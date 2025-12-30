using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Asp.Versioning;
using IntelliPM.Application.Identity.Commands;
using IntelliPM.Application.Identity.Queries;
using IntelliPM.Application.Common.Exceptions;
using IntelliPM.Domain.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace IntelliPM.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[EnableRateLimiting("auth")] // Strict rate limit for authentication endpoints
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController>? _logger;

    public AuthController(IMediator mediator, IWebHostEnvironment environment, IConfiguration configuration, ILogger<AuthController>? logger = null)
    {
        _mediator = mediator;
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Public user registration endpoint (DEPRECATED - Registration is disabled)
    /// </summary>
    /// <remarks>
    /// This endpoint is deprecated. Public registration is disabled. 
    /// Please contact your administrator for an invitation link.
    /// </remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    [Obsolete("Public registration is disabled. Please contact your administrator for an invitation.")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public IActionResult Register([FromBody] RegisterRequest req)
        {
        return StatusCode(403, new { 
            error = "Public registration is disabled. Please contact your administrator for an invitation." 
        });
    }

    /// <summary>
    /// Authenticate user and return JWT tokens
    /// </summary>
    /// <remarks>
    /// Authenticates a user with username and password. Returns JWT access and refresh tokens.
    /// In development mode, tokens are returned in the response body. In production, tokens are set as HTTP-only cookies.
    /// </remarks>
    /// <param name="req">Login credentials (username and password)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User information and tokens (development only)</returns>
    /// <response code="200">Login successful</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest req,
        CancellationToken ct)
    {
        var cmd = new LoginCommand(req.Username, req.Password);
        var result = await _mediator.Send(cmd, ct);
        
        // Determine if we should use Secure cookies
        // Use Secure = true if in production OR if request is over HTTPS (e.g., ngrok)
        var useSecureCookies = !_environment.IsDevelopment() || Request.IsHttps;
        
        // Set httpOnly cookie instead of returning token in body
        Response.Cookies.Append("auth_token", result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = useSecureCookies, // HTTPS only in production or when using HTTPS (ngrok)
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15), // Match JWT expiration
            Path = "/",
            Domain = null // Let browser handle domain
        });

        // Optional: Set refresh token cookie
        if (!string.IsNullOrEmpty(result.RefreshToken))
        {
            Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = useSecureCookies,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/"
            });
        }

        // Return user info
        var response = new
        {
            userId = result.UserId,
            username = result.Username,
            email = result.Email,
            roles = result.Roles,
            message = "Logged in successfully"
        };
        
        // In development, also return the token in the response body for easier testing (Swagger, etc.)
        // In production, tokens are only in httpOnly cookies for security
        if (_environment.IsDevelopment())
        {
            return Ok(new
            {
                userId = result.UserId,
                username = result.Username,
                email = result.Email,
                roles = result.Roles,
                accessToken = result.AccessToken, // For development/testing only
                refreshToken = result.RefreshToken, // For development/testing only
                message = "Logged in successfully"
            });
        }
        
        return Ok(response);
    }

    /// <summary>
    /// Refresh JWT access token using refresh token
    /// </summary>
    /// <remarks>
    /// Uses the refresh token from HTTP-only cookie to generate a new access token.
    /// The refresh token must be valid and not expired.
    /// </remarks>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="401">Refresh token not found or invalid</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        CancellationToken ct)
    {
        // Get refresh token from cookie
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { error = "Refresh token not found" });
        }

        var cmd = new RefreshTokenCommand(refreshToken);
        var result = await _mediator.Send(cmd, ct);
        
        // Determine if we should use Secure cookies
        var useSecureCookies = !_environment.IsDevelopment() || Request.IsHttps;
        
        // Set new access token cookie
        Response.Cookies.Append("auth_token", result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = useSecureCookies,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(15),
            Path = "/",
            Domain = null
        });

        // Update refresh token cookie if a new one was issued
        if (!string.IsNullOrEmpty(result.RefreshToken))
        {
            Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = useSecureCookies,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/"
            });
        }

        return Ok(new { message = "Token refreshed successfully" });
    }

    /// <summary>
    /// Logout user by clearing authentication cookies
    /// </summary>
    /// <remarks>
    /// Clears the auth_token and refresh_token HTTP-only cookies.
    /// This endpoint does not invalidate tokens on the server side (stateless JWT).
    /// </remarks>
    /// <returns>Success message</returns>
    /// <response code="200">Logged out successfully</response>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token");
        Response.Cookies.Delete("refresh_token");
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Request a password reset email
    /// </summary>
    /// <param name="request">Email or username</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success message (always returns success to prevent email enumeration)</returns>
    /// <response code="200">If account exists, password reset email has been sent</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RequestPasswordResetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var command = new RequestPasswordResetCommand(request.EmailOrUsername);
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error processing password reset request for {EmailOrUsername}", request.EmailOrUsername);
            // Always return success to prevent information disclosure
            return Ok(new RequestPasswordResetResponse(
                Success: true,
                Message: "If an account exists with that email or username, a password reset link has been sent."
            ));
        }
    }

    /// <summary>
    /// Reset password using a token from email
    /// </summary>
    /// <param name="request">Reset token and new password</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Password has been reset successfully</response>
    /// <response code="400">Bad request - Invalid token or validation failed</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ResetPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var command = new ResetPasswordCommand(request.Token, request.NewPassword, request.ConfirmPassword);
            var result = await _mediator.Send(command, ct);

            if (!result.Success)
            {
                return BadRequest(new { error = result.Message });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error resetting password");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An error occurred while resetting your password." });
        }
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    /// <remarks>
    /// Returns the current user's profile information based on the JWT token in the Authorization header or cookie.
    /// </remarks>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Current user information</returns>
    /// <response code="200">User information retrieved successfully</response>
    /// <response code="401">Authentication required</response>
    /// <response code="404">User not found</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        try
        {
            var userId = GetCurrentUserId();
            var query = new GetCurrentUserQuery(userId);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger?.LogWarning(ex, "Unauthorized access to GetMe endpoint");
            return Unauthorized(new { error = "Authentication required. Please log in again." });
        }
        catch (NotFoundException ex)
        {
            _logger?.LogWarning(ex, "User not found in GetMe endpoint");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting current user");
            return Problem(
                title: "Error retrieving user information",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Invite a user to the system
    /// </summary>
    [Authorize]
    [HttpPost("invite")]
    [ProducesResponseType(typeof(InviteUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Invite(
        [FromBody] InviteUserRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized(new { error = "User ID not found in claims" });
            }

            var cmd = new InviteUserCommand(
                request.Email,
                request.GlobalRole,
                request.ProjectId,
                currentUserId
            );
            
            var result = await _mediator.Send(cmd, ct);
            
            return Ok(new
            {
                invitationId = result.InvitationId,
                email = result.Email,
                token = result.Token,
                expiresAt = result.ExpiresAt,
                message = "Invitation sent successfully"
            });
        }
        catch (UnauthorizedException ex)
        {
            return Problem(
                title: "Forbidden",
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden
            );
        }
        catch (NotFoundException ex)
        {
            return Problem(
                title: "Not Found",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "Bad Request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Error sending invitation",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Validate an invitation token
    /// </summary>
    [AllowAnonymous]
    [HttpGet("invite/{token}")]
    [ProducesResponseType(typeof(ValidateInviteTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateInviteToken(
        string token,
        CancellationToken ct = default)
    {
        try
        {
            var query = new ValidateInviteTokenQuery(token);
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return Problem(
                title: "Invalid Token",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "Invalid Invitation",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Error validating invitation",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    /// <summary>
    /// Accept an invitation and create user account
    /// </summary>
    [AllowAnonymous]
    [HttpPost("invite/accept")]
    [ProducesResponseType(typeof(AcceptInviteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptInvite(
        [FromBody] AcceptOrganizationInviteRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var cmd = new AcceptOrganizationInviteCommand(
                request.Token,
                request.Username,
                request.Password,
                request.ConfirmPassword
            );
            var result = await _mediator.Send(cmd, ct);

            // Determine if we should use Secure cookies
            var useSecureCookies = !_environment.IsDevelopment() || Request.IsHttps;

            // Set httpOnly cookie for access token
            Response.Cookies.Append("auth_token", result.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = useSecureCookies,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15),
                Path = "/",
                Domain = null
            });

            // Set refresh token cookie
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = useSecureCookies,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                    Path = "/"
                });
            }

            return Ok(new
            {
                userId = result.UserId,
                username = result.Username,
                email = result.Email,
                message = "Account created successfully"
            });
        }
        catch (NotFoundException ex)
        {
            return Problem(
                title: "Invalid Token",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                title: "Invalid Invitation",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Error accepting invitation",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}

public record RegisterRequest(string Username, string Email, string Password, string FirstName, string LastName);
public record LoginRequest(string Username, string Password);
public record RefreshRequest(string RefreshToken);
public record InviteUserRequest(string Email, GlobalRole GlobalRole, int? ProjectId = null);
public record AcceptInviteRequest(string Token, string Password, string FirstName, string LastName);
public record AcceptOrganizationInviteRequest(string Token, string Username, string Password, string ConfirmPassword);
public record ForgotPasswordRequest(string EmailOrUsername);
public record ResetPasswordRequest(string Token, string NewPassword, string ConfirmPassword);

