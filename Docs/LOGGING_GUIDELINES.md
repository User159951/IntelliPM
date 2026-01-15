# Logging Guidelines for IntelliPM

## Overview

IntelliPM uses **Serilog** for structured logging with enterprise-grade observability features including:
- **Log Correlation**: Every request has a unique correlation ID for tracing
- **Organization Scoping**: All logs automatically include organization context
- **PII Protection**: Automatic redaction of sensitive data
- **Structured Logging**: JSON-formatted logs for easy querying

## Log Correlation

### Correlation IDs

Every HTTP request automatically receives a correlation ID that:
- Is generated if not provided in the `X-Correlation-ID` header
- Is included in all logs within the request scope
- Is returned in the `X-Correlation-ID` response header
- Is propagated to external services via HTTP headers

### Using Correlation IDs

```csharp
// Correlation ID is automatically available in all logs
_logger.LogInformation("Processing request for user {UserId}", userId);
// Log output will include: CorrelationId: "abc-123-def-456"

// Access correlation ID programmatically
var correlationId = _currentUserService.GetCorrelationId();
```

### Passing Correlation IDs to External Services

The `CorrelationIdHttpMessageHandler` automatically adds correlation IDs to outgoing HTTP requests:

```csharp
// HttpClient configured with CorrelationIdHttpMessageHandler
// automatically includes X-Correlation-ID header
var response = await _httpClient.GetAsync("https://external-api.com/data");
```

## Organization Scoping

### Automatic Organization Context

All logs automatically include:
- `OrganizationId`: Current user's organization ID (if authenticated)
- `UserId`: Current user's ID (if authenticated)
- `RequestPath`: The HTTP request path

These are added via `LoggingScopeMiddleware` which runs after authentication.

### Example Log Output

```json
{
  "Timestamp": "2024-01-15T10:30:45.123Z",
  "Level": "Information",
  "Message": "Task created successfully",
  "CorrelationId": "abc-123-def-456",
  "OrganizationId": 42,
  "UserId": 100,
  "RequestPath": "/api/v1/Tasks",
  "TaskId": 500
}
```

## Structured Logging

### Best Practices

Always use structured logging with named parameters:

```csharp
// ✅ GOOD: Structured logging with named parameters
_logger.LogInformation(
    "AI agent executed: {AgentType} for Project {ProjectId} | Duration: {Duration}ms",
    agentType,
    projectId,
    duration
);

// ❌ BAD: String interpolation
_logger.LogInformation($"AI agent executed: {agentType} for Project {projectId}");
```

### Log Levels

- **Debug**: Detailed diagnostic information (development only)
- **Information**: General application flow and important events
- **Warning**: Unexpected situations that don't stop execution
- **Error**: Exceptions and errors that require attention
- **Critical**: System failures requiring immediate action

### Example Usage

```csharp
// Information: Normal operation
_logger.LogInformation("User {UserId} created task {TaskId} in project {ProjectId}",
    userId, taskId, projectId);

// Warning: Recoverable issues
_logger.LogWarning("Rate limit approaching for user {UserId}. Current: {CurrentCount}/{Limit}",
    userId, currentCount, limit);

// Error: Exceptions
_logger.LogError(ex, "Failed to process payment for order {OrderId}", orderId);

// Critical: System failures
_logger.LogCritical("Database connection lost. Attempting reconnection...");
```

## PII Protection

### Automatic Redaction

The `PiiRedactor` utility automatically redacts:
- **Passwords**: Fields containing "password", "passwd", "pwd"
- **Tokens**: Fields containing "token", "access_token", "refresh_token", "api_key"
- **Email Addresses**: Detected via regex pattern
- **Credit Cards**: Detected via pattern matching
- **SSN**: Social Security Numbers
- **Other Sensitive Fields**: Based on field name keywords

### Using PiiRedactor

```csharp
using IntelliPM.Infrastructure.Utilities;

// Redact sensitive data from strings
var sanitized = PiiRedactor.Redact(password, "password");
// Returns: "[REDACTED]"

// Sanitize objects before logging
var sanitizedDto = PiiRedactor.SanitizeObject(userDto);
_logger.LogInformation("User data: {@UserData}", sanitizedDto);

// Mark properties as sensitive
public class LoginRequest
{
    public string Username { get; set; }
    
    [Sensitive]
    public string Password { get; set; }
}
```

### Never Log These Directly

❌ **NEVER** log:
- Passwords (even hashed)
- JWT tokens or API keys
- Credit card numbers
- Social Security Numbers
- Full email addresses (use redaction)
- Personal identification numbers

✅ **ALWAYS** sanitize before logging:
```csharp
// ❌ BAD
_logger.LogInformation("User login: {Username}, Password: {Password}", username, password);

// ✅ GOOD
_logger.LogInformation("User login attempt: {Username}", username);
// Or if you must log the request object:
var sanitized = PiiRedactor.SanitizeObject(loginRequest);
_logger.LogInformation("Login request: {@Request}", sanitized);
```

## Logging in Services

### Dependency Injection

Inject `ILogger<T>` into your services:

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }
}
```

### Including Context

Always include relevant context in logs:

```csharp
// Include correlation ID, user, and organization context
_logger.LogInformation(
    "Processing AI decision {DecisionId} for project {ProjectId} | " +
    "Agent: {AgentType} | Duration: {Duration}ms",
    decisionId,
    projectId,
    agentType,
    duration
);
```

## Log Aggregation

### Seq (Development/Staging)

Configure via environment variables:
```bash
SEQ_URL=http://localhost:5341
SEQ_API_KEY=your-api-key
```

### Production Log Sinks

- **Console**: Structured JSON output
- **File**: Rolling daily logs (`logs/intellipm-YYYYMMDD.txt`)
- **Seq/ELK**: Centralized log aggregation (configured via env vars)

### Querying Logs

In Seq or ELK, you can query by:
- `CorrelationId = "abc-123-def-456"` - Find all logs for a request
- `OrganizationId = 42` - Find all logs for an organization
- `UserId = 100` - Find all logs for a user
- `RequestPath = "/api/v1/Tasks"` - Find all logs for an endpoint

## Testing

### Integration Tests

See `LoggingObservabilityTests.cs` for examples:
- Verifying correlation IDs in response headers
- Testing organization context in logs
- Validating correlation ID propagation

### Unit Tests

See `PiiRedactorTests.cs` for PII redaction tests.

## Configuration

### appsettings.json

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithProcessId",
      "WithThreadId"
    ],
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/intellipm-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  }
}
```

### Environment Variables

- `SEQ_URL`: Seq server URL (optional)
- `SEQ_API_KEY`: Seq API key (optional)

## Best Practices Summary

1. ✅ **Always use structured logging** with named parameters
2. ✅ **Include correlation ID** in logs (automatic via middleware)
3. ✅ **Include organization context** (automatic via middleware)
4. ✅ **Sanitize sensitive data** before logging
5. ✅ **Use appropriate log levels** (Debug, Information, Warning, Error, Critical)
6. ✅ **Include relevant context** (IDs, durations, counts, etc.)
7. ❌ **Never log passwords, tokens, or PII** directly
8. ❌ **Avoid string interpolation** in log messages
9. ❌ **Don't log entire request/response objects** without sanitization

## Troubleshooting

### Logs Missing Correlation ID

- Ensure `CorrelationIdMiddleware` is registered early in the pipeline
- Check that `LogContext.PushProperty` is being used correctly

### Logs Missing Organization Context

- Ensure `LoggingScopeMiddleware` runs after `UseAuthentication()`
- Verify user is authenticated (OrganizationId is 0 for unauthenticated users)

### PII Leaking in Logs

- Use `PiiRedactor.SanitizeObject()` before logging DTOs
- Mark sensitive properties with `[Sensitive]` attribute
- Review log output in staging before production deployment

## Additional Resources

- [Serilog Documentation](https://serilog.net/)
- [Seq Documentation](https://docs.datalust.co/docs)
- [Structured Logging Best Practices](https://www.elastic.co/guide/en/ecs/current/index.html)
