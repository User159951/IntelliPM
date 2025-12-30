using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IntelliPM.Infrastructure.Persistence;

/// <summary>
/// Factory for creating AppDbContext at design-time (for migrations)
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Determine environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        
        // Build configuration
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../IntelliPM.API"));
        
        // Load base appsettings first
        configurationBuilder.AddJsonFile("appsettings.json", optional: false);
        
        // Load environment-specific appsettings (will override base settings)
        if (!string.IsNullOrEmpty(environment))
        {
            configurationBuilder.AddJsonFile($"appsettings.{environment}.json", optional: true);
        }
        
        // Load User Secrets for Development environment (important for design-time migrations)
        // Using the UserSecretsId from IntelliPM.API project: c4ff6d34-ce57-4953-b620-a598acce6315
        if (environment == "Development")
        {
            configurationBuilder.AddUserSecrets("c4ff6d34-ce57-4953-b620-a598acce6315");
        }
        
        var configuration = configurationBuilder.Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("SqlServer") 
                            ?? configuration.GetConnectionString("DefaultConnection")
                            ?? "Server=localhost,1433;Database=IntelliPM;User Id=sa;Password=YourPassword123!;Encrypt=True;TrustServerCertificate=True;";

        // Build DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}

