using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using IntelliPM.Application.Common.Interfaces;
using IntelliPM.Application.Abstractions.VectorStore;
using IntelliPM.Application.Interfaces;
using IntelliPM.Application.Interfaces.Services;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Identity;
using IntelliPM.Infrastructure.VectorStore;
using IntelliPM.Infrastructure.LLM;
using IntelliPM.Infrastructure.AI.Services;
using IntelliPM.Infrastructure.Services;
using IntelliPM.Infrastructure.BackgroundServices;

namespace IntelliPM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // EF Core - SQL Server (Transactional data)
        // Register HttpContextAccessor for CurrentUserService
        services.AddHttpContextAccessor();
        
        // Register CurrentUserService
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        // Register PermissionService
        services.AddScoped<IPermissionService, PermissionService>();

        // Register OrganizationScopingService
        services.AddScoped<IntelliPM.Application.Common.Services.OrganizationScopingService>();
        
        // Register OrganizationPermissionPolicyService
        services.AddScoped<IntelliPM.Application.Common.Services.OrganizationPermissionPolicyService>();
        
        // Register DbContext with service provider injection for CurrentUserService
        services.AddDbContext<AppDbContext>((serviceProvider, opts) =>
        {
            opts.UseSqlServer(
                config.GetConnectionString("SqlServer") ?? config.GetConnectionString("DefaultConnection"),
                sqlOpts => sqlOpts.EnableRetryOnFailure(3)
            );
        });
        
        // Override DbContext registration to inject service provider
        services.AddScoped<AppDbContext>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>();
            return new AppDbContext(options, serviceProvider);
        });

        // EF Core - PostgreSQL + pgvector (Agent memory)
        services.AddDbContext<VectorDbContext>(opts =>
            opts.UseNpgsql(
                config.GetConnectionString("VectorDb"),
                npgsqlOpts =>
                {
                    npgsqlOpts.UseVector();
                    npgsqlOpts.EnableRetryOnFailure(3);
                }
            ));

        // UnitOfWork & Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Auth
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<PasswordHasher>(); // Keep for backward compatibility
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        // Vector Store (legacy interface - used by existing agents)
        services.AddScoped<IVectorStore, PostgresVectorStore>();

        // Vector Memory Store (new Clean Architecture interface - uses EF Core)
        services.AddScoped<IVectorMemoryStore, VectorMemoryStorePgvector>();

        // HTTP Client Factory (for health checks and LLM)
        services.AddHttpClient();

        // LLM (Ollama)
        services.AddHttpClient<ILlmClient, OllamaClient>(client =>
        {
            client.BaseAddress = new Uri(config["Ollama:Endpoint"] ?? "http://localhost:11434");
            client.Timeout = TimeSpan.FromMinutes(2); // LLM calls can be slow
        });

        // AI Agent Service (using Semantic Kernel with automatic function calling)
        services.AddScoped<IAgentService, SemanticKernelAgentService>();

        // Cache Service
        services.AddSingleton<ICacheService, CacheService>();

        // Feature Flag Service
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();

        // Email Template Service
        services.AddScoped<EmailTemplateService>();

        // Email Service - Use SMTP if configured, otherwise use stub EmailService
        var emailProvider = config["Email:Provider"];
        if (emailProvider == "SMTP" && !string.IsNullOrEmpty(config["Email:SmtpUsername"]))
        {
            services.AddScoped<IEmailService, SmtpEmailService>();
        }
        else
        {
        services.AddScoped<IEmailService, EmailService>();
        }

        // MediatR handlers that live in Infrastructure (Semantic Kernel agents)
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // Seeders
        services.AddScoped<DataSeeder>();
        services.AddScoped<MultiOrgDataSeeder>();

        // Background Services
        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<MilestoneStatusUpdater>();


        return services;
    }
}

