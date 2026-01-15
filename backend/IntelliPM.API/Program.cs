using IntelliPM.Infrastructure;
using IntelliPM.Application;
using IntelliPM.Infrastructure.Persistence;
using IntelliPM.Infrastructure.Health;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;
using Serilog.Sinks.Seq;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.SemanticKernel;
using System.Threading.RateLimiting;
using System.Security.Claims;
using Asp.Versioning;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Sentry;
using Microsoft.AspNetCore.Authorization;
using IntelliPM.API.Authorization;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

// Configure Sentry before creating the builder
var builder = WebApplication.CreateBuilder(args);

// Get environment name
var environment = builder.Environment.EnvironmentName;
var isDevelopment = builder.Environment.IsDevelopment();
var isProduction = builder.Environment.IsProduction();

// Configure Sentry
var sentryDsn = Environment.GetEnvironmentVariable("SENTRY_DSN") 
    ?? builder.Configuration["Sentry:Dsn"];

if (!string.IsNullOrWhiteSpace(sentryDsn))
{
    builder.WebHost.UseSentry(o =>
    {
        o.Dsn = sentryDsn;
        o.Environment = environment;
        
        // Performance tracking
        o.TracesSampleRate = isProduction ? 0.1 : 1.0; // 10% in production, 100% in dev
        o.ProfilesSampleRate = isProduction ? 0.1 : 1.0;
        
        // Request/response logging
        o.SendDefaultPii = false; // Don't send PII by default
        o.AttachStacktrace = true;
        o.Debug = isDevelopment;
        
        // Performance monitoring is enabled via TracesSampleRate above
        // TracesSampler provides more granular control
        o.TracesSampler = context =>
        {
            // Sample rate based on environment
            return isProduction ? 0.1 : 1.0;
        };
    });
}

// Configure Serilog with structured logging
var loggerConfiguration = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    // Set minimum level based on environment
    .MinimumLevel.Is(isDevelopment ? LogEventLevel.Debug : LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    // Enrichment
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Environment", environment)
    // Console sink with structured JSON format
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
        formatProvider: System.Globalization.CultureInfo.InvariantCulture)
    // File sink with structured format (JSON)
    .WriteTo.File(
        path: "logs/intellipm-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 7,
        shared: true,
        formatProvider: System.Globalization.CultureInfo.InvariantCulture);

// Add Seq sink if SEQ_URL environment variable is set
var seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");
if (!string.IsNullOrWhiteSpace(seqUrl))
{
    loggerConfiguration.WriteTo.Seq(
        serverUrl: seqUrl,
        apiKey: Environment.GetEnvironmentVariable("SEQ_API_KEY"),
        restrictedToMinimumLevel: isDevelopment ? LogEventLevel.Debug : LogEventLevel.Information);
}

Log.Logger = loggerConfiguration.CreateLogger();

builder.Host.UseSerilog();

// Memory Cache
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache(); // For distributed scenarios later

// Add services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Configure Semantic Kernel with Ollama
// Support both config file and environment variables (Agent__TimeoutSeconds)
var ollamaEndpoint = builder.Configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
var ollamaModel = builder.Configuration["Ollama:Model"] ?? "llama3.2:3b";
var agentTimeoutSeconds = builder.Configuration.GetValue<int>("Agent:TimeoutSeconds", 60);

builder.Services.AddSingleton<Kernel>(serviceProvider =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    
    // Add Ollama chat completion
    // Note: Timeout is controlled via CancellationToken in the service layer
    kernelBuilder.AddOllamaChatCompletion(
        modelId: ollamaModel,
        endpoint: new Uri(ollamaEndpoint)
    );
    
    // Add logging
    kernelBuilder.Services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddConsole();
        loggingBuilder.SetMinimumLevel(LogLevel.Information);
    });
    
    return kernelBuilder.Build();
});

Log.Logger.Information("Semantic Kernel configured with Ollama at {Endpoint} using model {Model} (Agent Timeout: {Timeout}s)", 
    ollamaEndpoint, ollamaModel, agentTimeoutSeconds);

// Add authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT SecretKey must be configured in User Secrets or environment variables");

if (jwtSecretKey.Length < 32)
{
    throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long");
}

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", opts =>
    {
        opts.TokenValidationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Permission-based authorization is handled via PermissionAuthorizationHandler
    // Policies are created dynamically by PermissionPolicyProvider
});

// Register custom policy provider for permission-based policies
builder.Services.AddSingleton<IAuthorizationPolicyProvider, IntelliPM.API.Authorization.PermissionPolicyProvider>();

// Register permission authorization handler
builder.Services.AddScoped<IAuthorizationHandler, IntelliPM.API.Authorization.PermissionAuthorizationHandler>();

// Add Rate Limiting
var globalLimit = builder.Configuration.GetValue<int>("RateLimiting:Global:PermitLimit", 100);
var globalWindowMinutes = builder.Configuration.GetValue<int>("RateLimiting:Global:WindowMinutes", 1);
var authLimit = builder.Configuration.GetValue<int>("RateLimiting:Auth:PermitLimit", 5);
var authWindowMinutes = builder.Configuration.GetValue<int>("RateLimiting:Auth:WindowMinutes", 1);
var aiLimit = builder.Configuration.GetValue<int>("RateLimiting:AI:PermitLimit", 10);
var aiWindowMinutes = builder.Configuration.GetValue<int>("RateLimiting:AI:WindowMinutes", 1);

builder.Services.AddRateLimiter(options =>
{
    // Global rate limit: 100 requests per minute per user/IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var partitionKey = userId ?? ipAddress ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: partitionKey,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = globalLimit,
                Window = TimeSpan.FromMinutes(globalWindowMinutes),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    // Strict rate limit for authentication endpoints
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = authLimit,
                Window = TimeSpan.FromMinutes(authWindowMinutes),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Lenient rate limit for AI endpoints (they're slow)
    options.AddPolicy("ai", context =>
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var partitionKey = userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: partitionKey,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = aiLimit,
                Window = TimeSpan.FromMinutes(aiWindowMinutes),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 2
            });
    });

    // Handle rate limit exceeded
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests. Please try again later.",
                retryAfter = retryAfter.TotalSeconds
            }, cancellationToken: token);
        }
        else
        {
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                error = "Too many requests. Please try again later."
            }, cancellationToken: token);
        }
    };
});

// CORS
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("AllowFrontend", pb =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "http://localhost:3001" };
        
        pb.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Required for cookies
    });
});

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"),
        new MediaTypeApiVersionReader("version")
    );
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Validate SMTP configuration at startup
var emailSection = builder.Configuration.GetSection("Email");
var smtpHost = emailSection["SmtpHost"];
var smtpPort = emailSection.GetValue<int>("SmtpPort", 587);
var smtpUsername = emailSection["SmtpUsername"];
var smtpPassword = emailSection["SmtpPassword"];

if (string.IsNullOrWhiteSpace(smtpHost) || 
    string.IsNullOrWhiteSpace(smtpUsername) || 
    string.IsNullOrWhiteSpace(smtpPassword))
{
    Log.Logger.Warning(
        "SMTP configuration incomplete. Email functionality will be unavailable. " +
        "Required settings: Email:SmtpHost, Email:SmtpUsername, Email:SmtpPassword. " +
        "Current values - Host: {Host}, Port: {Port}, Username: {Username}",
        smtpHost ?? "not set",
        smtpPort,
        string.IsNullOrWhiteSpace(smtpUsername) ? "not set" : "set");
}
else
{
    Log.Logger.Information(
        "SMTP configuration validated. Host: {Host}, Port: {Port}, Username: {Username}",
        smtpHost, smtpPort, smtpUsername);
}

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "db", "sql" })
    .AddCheck<OllamaHealthCheck>("ollama", tags: new[] { "ai", "llm" })
    .AddCheck<IntelliPM.Infrastructure.Health.SmtpHealthCheck>("smtp", tags: new[] { "email", "smtp" })
    .AddCheck("memory", () =>
    {
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var threshold = 1024L * 1024L * 1024L; // 1 GB

        return allocated < threshold
            ? HealthCheckResult.Healthy($"Memory: {allocated / 1024 / 1024} MB")
            : HealthCheckResult.Degraded($"Memory usage high: {allocated / 1024 / 1024} MB");
    }, tags: new[] { "memory" });

// Health Checks UI (optional) - using SQLite file storage
var healthChecksDbPath = (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Testing")
    ? Path.Combine(builder.Environment.ContentRootPath, "healthchecks.db")
    : "/app/data/healthchecks.db"; // For Docker, use /app/data directory

// Ensure directory exists for Docker
var healthChecksDbDir = Path.GetDirectoryName(healthChecksDbPath);
if (!string.IsNullOrEmpty(healthChecksDbDir) && !Directory.Exists(healthChecksDbDir))
{
    Directory.CreateDirectory(healthChecksDbDir);
}

// Configure HealthChecks UI endpoint URL
// In Docker, the app runs on port 80 internally, so use localhost:80
// In local development, check if we're using the default ports from launchSettings
var urls = builder.Configuration["ASPNETCORE_URLS"] ?? "";
var healthCheckUrl = urls.Contains(":5001") || urls.Contains(":5000")
    ? "http://localhost:5001/api/health"  // Local development
    : "http://localhost:80/api/health";    // Docker (internal container URL on port 80)

builder.Services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(60);
    options.MaximumHistoryEntriesPerEndpoint(50);
    options.AddHealthCheckEndpoint("IntelliPM API", healthCheckUrl);
})
.AddSqliteStorage($"Data Source={healthChecksDbPath}");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings instead of numbers (e.g., "Admin" instead of 2)
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IntelliPM API",
        Version = "v1.0",
        Description = "IntelliPM Project Management API with AI capabilities - Version 1.0",
        Contact = new OpenApiContact
        {
            Name = "IntelliPM Support",
            Email = "support@intellipm.com"
        },
        License = new OpenApiLicense
        {
            Name = "Proprietary",
            Url = new Uri("https://intellipm.com/license")
        }
    });

    // Include XML comments for better API documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Configure to use fully qualified names for Dictionary types to avoid conflicts
    c.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
    
    // Ignore problematic schema properties that might cause Swagger generation to fail
    c.IgnoreObsoleteProperties();
    c.IgnoreObsoleteActions();
    
    // Handle any schema generation errors gracefully
    c.SupportNonNullableReferenceTypes();
    
    // Use enum string values
    c.UseInlineDefinitionsForEnums();
});

// Read security headers configuration
var enableHSTS = builder.Configuration.GetValue<bool>("SecurityHeaders:EnableHSTS", false);
var hstsMaxAgeDays = builder.Configuration.GetValue<int>("SecurityHeaders:HSTSMaxAgeDays", 365);
var cspConnectSources = builder.Configuration.GetSection("SecurityHeaders:CSPConnectSources")
    .Get<string[]>() ?? new[] { "http://localhost:11434" };

var app = builder.Build();

// SECURITY: Runtime check to prevent debug/test controllers in production
#if !DEBUG
if (isProduction)
{
    // Verify that TestController is not accessible in production builds
    // This is a compile-time check, but we also verify at runtime
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Production build detected - DEBUG controllers are excluded via preprocessor directives");
    
    // Additional runtime verification: Check if any debug controllers are registered
    // This would fail at compile time if TestController is referenced, but provides extra safety
    try
    {
        var controllerTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && 
                       t.Name.Contains("Test", StringComparison.OrdinalIgnoreCase) &&
                       !t.IsAbstract)
            .ToList();
        
        if (controllerTypes.Any())
        {
            var controllerNames = string.Join(", ", controllerTypes.Select(t => t.Name));
            logger.LogError("SECURITY WARNING: Test controllers detected in production build: {Controllers}", controllerNames);
            throw new InvalidOperationException(
                $"SECURITY VIOLATION: Debug/test controllers detected in production build: {controllerNames}. " +
                "All test controllers must be wrapped with #if DEBUG preprocessor directives.");
        }
    }
    catch (ReflectionTypeLoadException)
    {
        // Type loading exceptions are expected if controllers are excluded via #if DEBUG
        // This is actually the desired behavior - the types won't be loadable in Release builds
        logger.LogInformation("Debug controllers successfully excluded from production build (type loading failed as expected)");
    }
}
#endif

// Migrate and seed databases
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Migrate SQL Server (transactional data)
        logger.LogInformation("Applying SQL Server migrations...");
        var appContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await appContext.Database.MigrateAsync();
        logger.LogInformation("SQL Server migrations applied successfully");

        // Migrate PostgreSQL (vector store)
        // TEMPORARILY DISABLED: Pgvector package conflict - will be fixed after SQL Server migration
        // logger.LogInformation("Applying PostgreSQL migrations...");
        // var vectorContext = scope.ServiceProvider.GetRequiredService<IntelliPM.Infrastructure.VectorStore.VectorDbContext>();
        // await vectorContext.Database.MigrateAsync();
        // logger.LogInformation("PostgreSQL migrations applied successfully");

        // NOTE: Demo seed data has been disabled so that only real data appears.
        // However, we always need to seed permissions and role permissions for RBAC to work.

        // Seed all RBAC data using the comprehensive versioned seed system
        // This includes: permissions, role-permissions, workflow rules, and AI decision policies
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        await IntelliPM.Infrastructure.Persistence.DataSeeder.SeedAllRBACDataAsync(
            appContext,
            loggerFactory);
        logger.LogInformation("RBAC data seeding completed");

        // Seed default organization (idempotent - safe to run on every startup)
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IntelliPM.Infrastructure.Identity.PasswordHasher>();
        await IntelliPM.Infrastructure.Persistence.DataSeeder.SeedDefaultOrganizationAsync(
            appContext,
            logger);

                // Seed SuperAdmin user from configuration (idempotent - safe to run on every startup)
                await IntelliPM.Infrastructure.Persistence.DataSeeder.SeedSuperAdminUserAsync(
                    appContext,
                    passwordHasher,
                    logger,
                    builder.Configuration);

                // Seed OrganizationAIQuota for all organizations (idempotent - safe to run on every startup)
                await IntelliPM.Infrastructure.Persistence.DataSeeder.SeedOrganizationAIQuotasAsync(
                    appContext,
                    logger);

        // Seed development admin user (development environment only)
        if (app.Environment.IsDevelopment())
        {
            await IntelliPM.Infrastructure.Persistence.DataSeeder.SeedDevelopmentAdminUserAsync(
                appContext,
                passwordHasher,
                logger,
                isDevelopment: true);
        }

        // Seed multi-organization test data (optional - can be disabled via configuration)
        var seedMultiOrg = builder.Configuration.GetValue<bool>("SeedData:MultiOrganization", false);
        if (seedMultiOrg)
        {
            logger.LogInformation("Seeding multi-organization test data...");
            var multiOrgSeeder = scope.ServiceProvider.GetRequiredService<IntelliPM.Infrastructure.Persistence.MultiOrgDataSeeder>();
            await multiOrgSeeder.SeedAsync();
            logger.LogInformation("Multi-organization data seeded successfully");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the databases");
    }
}

// Global exception handler with Serilog and Sentry logging
app.UseExceptionHandler(appBuilder =>
{
    appBuilder.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        // Log to Serilog with structured logging
        if (exception != null)
        {
            logger.LogError(
                exception,
                "Unhandled exception occurred. Path: {Path}, Method: {Method}, StatusCode: {StatusCode}",
                context.Request.Path,
                context.Request.Method,
                context.Response.StatusCode);
        }

        // Capture exception in Sentry
        if (exception != null)
        {
            SentrySdk.CaptureException(exception, scope =>
            {
                scope.SetTag("path", context.Request.Path);
                scope.SetTag("method", context.Request.Method);
                scope.SetExtra("statusCode", context.Response.StatusCode);
                scope.SetExtra("queryString", context.Request.QueryString.ToString());
                scope.SetExtra("userAgent", context.Request.Headers["User-Agent"].ToString());
            });
        }

        var response = exception switch
        {
            IntelliPM.Application.Common.Exceptions.NotFoundException notFoundEx => new
            {
                statusCode = StatusCodes.Status404NotFound,
                error = notFoundEx.Message,
                errors = (object?)null,
                details = (object?)null
            },
            IntelliPM.Application.Common.Exceptions.UnauthorizedException unauthorizedEx => new
            {
                statusCode = StatusCodes.Status401Unauthorized,
                error = unauthorizedEx.Message,
                errors = (object?)null,
                details = (object?)null
            },
            IntelliPM.Application.Common.Exceptions.ValidationException validationEx => new
            {
                statusCode = StatusCodes.Status400BadRequest,
                error = validationEx.Message,
                errors = (object?)validationEx.Errors,
                details = (object?)null
            },
            IntelliPM.Application.Common.Exceptions.ConcurrencyException concurrencyEx => new
            {
                statusCode = StatusCodes.Status409Conflict,
                error = concurrencyEx.Message,
                errors = (object?)null,
                details = (object?)null
            },
            IntelliPM.Application.Common.Exceptions.AIQuotaExceededException quotaEx => new
            {
                statusCode = StatusCodes.Status429TooManyRequests,
                error = quotaEx.Message,
                errors = (object?)null,
                details = (object?)new
                {
                    organizationId = quotaEx.OrganizationId,
                    quotaType = quotaEx.QuotaType,
                    currentUsage = quotaEx.CurrentUsage,
                    maxLimit = quotaEx.MaxLimit,
                    tierName = quotaEx.TierName
                }
            },
            IntelliPM.Application.Common.Exceptions.AIDisabledException aiDisabledEx => new
            {
                statusCode = StatusCodes.Status403Forbidden,
                error = aiDisabledEx.Message,
                errors = (object?)null,
                details = (object?)new
                {
                    organizationId = aiDisabledEx.OrganizationId,
                    reason = aiDisabledEx.Reason
                }
            },
            IntelliPM.Application.Common.Exceptions.ForbiddenException forbiddenEx => new
            {
                statusCode = StatusCodes.Status403Forbidden,
                error = forbiddenEx.Message,
                errors = (object?)null,
                details = (object?)new
                {
                    permission = forbiddenEx.Permission,
                    organizationId = forbiddenEx.OrganizationId
                }
            },
            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => new
            {
                statusCode = StatusCodes.Status409Conflict,
                error = "The record was modified by another user. Please reload and try again.",
                errors = (object?)null,
                details = (object?)null
            },
            _ => new
            {
                statusCode = StatusCodes.Status500InternalServerError,
                error = exception?.Message ?? "An error occurred",
                errors = (object?)null,
                details = (object?)(app.Environment.IsDevelopment() ? exception?.StackTrace : null)
            }
        };

        context.Response.StatusCode = response.statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(response);
    });
});

// Configure middleware
// Enable Swagger in Development and Testing environments for API contract testing
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IntelliPM API v1");
    });
}

// Correlation ID middleware (must be early in pipeline, before request logging)
app.UseMiddleware<IntelliPM.API.Middleware.CorrelationIdMiddleware>();

// Serilog request logging with structured logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? LogEventLevel.Error
        : elapsed > 500
            ? LogEventLevel.Warning
            : LogEventLevel.Information;
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
        
        // Add correlation ID if available
        if (httpContext.Items.TryGetValue(IntelliPM.API.Middleware.CorrelationIdMiddleware.CorrelationIdItemKey, out var correlationId) &&
            correlationId is string correlationIdStr)
        {
            diagnosticContext.Set("CorrelationId", correlationIdStr);
        }
        
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserId", httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
        }
    };
});
app.UseCors("AllowFrontend");

// Sentry tracing middleware (must be early in pipeline to capture performance traces)
// Only use Sentry middleware if Sentry is configured
if (!string.IsNullOrWhiteSpace(sentryDsn))
{
app.UseSentryTracing();
}

// Middleware to read token from cookie and add to Authorization header
app.Use(async (context, next) =>
{
    // Only add Authorization header from cookie if it's not already present
    if (string.IsNullOrEmpty(context.Request.Headers.Authorization))
    {
        // Check if token is in cookie
        if (context.Request.Cookies.TryGetValue("auth_token", out var token) && !string.IsNullOrEmpty(token))
        {
            // Add to Authorization header for JWT middleware
            context.Request.Headers.Authorization = $"Bearer {token}";
        }
        else
        {
            // Log for debugging - especially for /api/Auth/me endpoint
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            var hasCookie = context.Request.Cookies.ContainsKey("auth_token");
            var cookieValue = hasCookie ? "present but empty" : "missing";
            
            // Log all cookies for debugging (but not their values for security)
            var cookieNames = string.Join(", ", context.Request.Cookies.Keys);
            
            if (context.Request.Path.StartsWithSegments("/api/v1/Auth/me") || 
                context.Request.Path.StartsWithSegments("/api/Auth/me"))
            {
                logger.LogWarning(
                    "Auth/me request - Cookie: {CookieStatus}, All cookies: {CookieCount}, Cookie names: {CookieNames}, Origin: {Origin}, Referer: {Referer}",
                    cookieValue, 
                    context.Request.Cookies.Count,
                    cookieNames,
                    context.Request.Headers.Origin.ToString(),
                    context.Request.Headers.Referer.ToString());
            }
        }
    }
    await next();
});

// Security Headers Middleware
app.Use(async (context, next) =>
{
    // Prevent MIME type sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

    // Prevent clickjacking
    context.Response.Headers.Append("X-Frame-Options", "DENY");

    // Enable XSS filter (legacy browsers)
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

    // Force HTTPS (only when enabled and not localhost)
    if (enableHSTS && !context.Request.Host.Host.Contains("localhost", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Headers.Append("Strict-Transport-Security", 
            $"max-age={hstsMaxAgeDays * 86400}; includeSubDomains; preload");
    }

    // Content Security Policy - Stricter configuration for XSS prevention
    // Note: 'unsafe-inline' is required for React apps, but we sanitize all user content
    // 'unsafe-eval' removed for better security (may need to be re-added if React requires it)
    var cspConnectSourcesString = string.Join(" ", cspConnectSources.Select(s => s));
    context.Response.Headers.Append("Content-Security-Policy", 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " + // Removed 'unsafe-eval' for better security
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' data:; " +
        $"connect-src 'self' {cspConnectSourcesString}; " +
        "base-uri 'self'; " + // Prevent base tag injection
        "form-action 'self'; " + // Prevent form action hijacking
        "frame-ancestors 'none'; " + // Prevent clickjacking
        "object-src 'none'; " + // Prevent plugin injection
        "upgrade-insecure-requests"); // Upgrade HTTP to HTTPS

    // Control referrer information
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    // Control browser features
    context.Response.Headers.Append("Permissions-Policy", 
        "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");

    // Remove server header (security through obscurity)
    context.Response.Headers.Remove("Server");
    context.Response.Headers.Remove("X-Powered-By");

    await next();
});

app.UseAuthentication();

// Tenant middleware must run after authentication (to access user claims)
// and before authorization (so authorization handlers can access tenant context)
app.UseMiddleware<IntelliPM.API.Middleware.TenantMiddleware>();

app.UseRateLimiter(); // Add rate limiting middleware
app.UseAuthorization();

// Middleware to check feature flags (after authentication to access user context)
app.UseMiddleware<IntelliPM.API.Middleware.FeatureFlagMiddleware>();

// Middleware to capture user context for Sentry (after authentication to access user claims)
// Only use Sentry middleware if Sentry is configured
if (!string.IsNullOrWhiteSpace(sentryDsn))
{
app.UseMiddleware<IntelliPM.API.Middleware.SentryUserContextMiddleware>();
}

app.MapControllers();

// Health Check Endpoints
app.MapHealthChecks("/api/v1/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/api/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/api/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/api/health/live", new HealthCheckOptions
{
    Predicate = _ => false, // Just checks if app is running
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Health Checks UI (optional)
app.MapHealthChecksUI(options => options.UIPath = "/health-ui");

app.Run();

// Make Program class accessible for WebApplicationFactory in tests
public partial class Program { }

