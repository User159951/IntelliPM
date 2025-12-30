using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.API.Controllers;

/// <summary>
/// TEMPORARY controller for generating password hashes for migrations and debugging.
/// This should be removed after generating the admin user migration.
/// </summary>
[ApiController]
[Route("api/dev/hash")]
public class AdminHashGeneratorController : ControllerBase
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly AppDbContext _context;
    
    public AdminHashGeneratorController(IPasswordHasher passwordHasher, AppDbContext context)
    {
        _passwordHasher = passwordHasher;
        _context = context;
    }
    
    /// <summary>
    /// Generate hash and salt for a password (default: "Admin@123456").
    /// Copy the returned values into the migration file.
    /// </summary>
    [HttpGet("generate")]
    public IActionResult GenerateHash([FromQuery] string password = "Admin@123456")
    {
        var result = _passwordHasher.HashPassword(password);
        return Ok(new { 
            hash = result.Hash, 
            salt = result.Salt,
            password = password,
            note = "Copy the hash and salt values into the SeedAdminUser migration file. Then remove this controller."
        });
    }
    
    /// <summary>
    /// TEMPORARY: Check if admin user exists in database
    /// </summary>
    [HttpGet("check-admin")]
    public async Task<IActionResult> CheckAdminUser()
    {
        var adminUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == "admin" || u.Email == "admin@intellipm.local");
        
        if (adminUser == null)
        {
            return Ok(new { 
                exists = false,
                message = "Admin user not found in database. Run the migration first."
            });
        }
        
        // Test password verification
        var passwordTest = _passwordHasher.VerifyPassword("Admin@123456", adminUser.PasswordHash, adminUser.PasswordSalt);
        
        return Ok(new {
            exists = true,
            username = adminUser.Username,
            email = adminUser.Email,
            globalRole = adminUser.GlobalRole.ToString(),
            isActive = adminUser.IsActive,
            organizationId = adminUser.OrganizationId,
            passwordMatch = passwordTest,
            note = passwordTest ? "Password 'Admin@123456' matches the hash" : "Password 'Admin@123456' does NOT match the hash"
        });
    }
}

