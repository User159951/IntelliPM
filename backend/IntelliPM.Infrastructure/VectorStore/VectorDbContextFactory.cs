using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pgvector.EntityFrameworkCore;

namespace IntelliPM.Infrastructure.VectorStore;

/// <summary>
/// Factory for creating VectorDbContext at design-time (for migrations)
/// </summary>
public class VectorDbContextFactory : IDesignTimeDbContextFactory<VectorDbContext>
{
    public VectorDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../IntelliPM.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("VectorDb")
                            ?? "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=intellipm_vector;Pooling=true;Maximum Pool Size=20;";

        // Build DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<VectorDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOpts => npgsqlOpts.UseVector());

        return new VectorDbContext(optionsBuilder.Options);
    }
}

