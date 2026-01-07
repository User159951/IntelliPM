# IntelliPM Backend Configuration Guide

This document describes the configuration options available in `appsettings.json` and environment variables.

## Table of Contents

- [Connection Strings](#connection-strings)
- [JWT Authentication](#jwt-authentication)
- [Email Configuration (SMTP)](#email-configuration-smtp)
- [Ollama AI Configuration](#ollama-ai-configuration)
- [Rate Limiting](#rate-limiting)
- [Security Headers](#security-headers)
- [Sentry Error Tracking](#sentry-error-tracking)
- [File Storage](#file-storage)
- [Environment Variables](#environment-variables)

## Connection Strings

### SQL Server
```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=sqlserver,1433;Database=IntelliPM;User Id=sa;Password=PLACEHOLDER;Encrypt=True;TrustServerCertificate=True;",
    "DefaultConnection": "Server=sqlserver,1433;Database=IntelliPM;User Id=sa;Password=PLACEHOLDER;Encrypt=True;TrustServerCertificate=True;"
  }
}
```

**Environment Variable:** `ConnectionStrings__SqlServer` or `ConnectionStrings__DefaultConnection`

### PostgreSQL (Vector Store)
```json
{
  "ConnectionStrings": {
    "VectorDb": "Host=postgres;Port=5432;Username=postgres;Password=PLACEHOLDER;Database=intellipm_vector;Pooling=true;Maximum Pool Size=20;"
  }
}
```

**Environment Variable:** `ConnectionStrings__VectorDb`

## JWT Authentication

```json
{
  "Jwt": {
    "SecretKey": "YOUR_SECRET_KEY_HERE",
    "Issuer": "IntelliPM",
    "Audience": "IntelliPM"
  }
}
```

**Required:** `SecretKey` must be at least 32 characters long.

**Environment Variables:**
- `Jwt__SecretKey`
- `Jwt__Issuer`
- `Jwt__Audience`

## Email Configuration (SMTP)

SMTP configuration is validated at startup. If not configured, email functionality will be unavailable but the application will continue to run.

```json
{
  "Email": {
    "Provider": "SMTP",
    "SmtpHost": "smtp-relay.brevo.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-username",
    "SmtpPassword": "your-password",
    "FromEmail": "noreply@intellipm.com",
    "FromName": "IntelliPM",
    "EnableSsl": true,
    "SecureSocketOptions": "StartTls"
  }
}
```

**Required Fields:**
- `SmtpHost` - SMTP server hostname
- `SmtpPort` - SMTP server port (typically 587 for TLS, 465 for SSL)
- `SmtpUsername` - SMTP authentication username
- `SmtpPassword` - SMTP authentication password

**Optional Fields:**
- `FromEmail` - Default sender email address (default: "noreply@intellipm.com")
- `FromName` - Default sender name (default: "IntelliPM")
- `EnableSsl` - Enable SSL/TLS (default: true)
- `SecureSocketOptions` - Override socket options: `Auto`, `None`, `StartTls`, `StartTlsWhenAvailable`, `SslOnConnect`

**Environment Variables:**
- `Email__SmtpHost`
- `Email__SmtpPort`
- `Email__SmtpUsername`
- `Email__SmtpPassword`
- `Email__FromEmail`
- `Email__FromName`

**Health Check:** SMTP connectivity is checked at `/api/v1/health` endpoint.

## Ollama AI Configuration

```json
{
  "Ollama": {
    "Endpoint": "http://ollama:11434",
    "Model": "llama3.2:3b"
  }
}
```

**Environment Variables:**
- `Ollama__Endpoint`
- `Ollama__Model`

## Rate Limiting

```json
{
  "RateLimiting": {
    "Global": {
      "PermitLimit": 100,
      "WindowMinutes": 1
    },
    "Auth": {
      "PermitLimit": 30,
      "WindowMinutes": 1
    },
    "AI": {
      "PermitLimit": 10,
      "WindowMinutes": 1
    }
  }
}
```

**Environment Variables:**
- `RateLimiting__Global__PermitLimit`
- `RateLimiting__Global__WindowMinutes`
- `RateLimiting__Auth__PermitLimit`
- `RateLimiting__Auth__WindowMinutes`
- `RateLimiting__AI__PermitLimit`
- `RateLimiting__AI__WindowMinutes`

## Security Headers

```json
{
  "SecurityHeaders": {
    "EnableHSTS": false,
    "HSTSMaxAgeDays": 365,
    "CSPConnectSources": [
      "http://localhost:11434"
    ]
  }
}
```

**Environment Variables:**
- `SecurityHeaders__EnableHSTS`
- `SecurityHeaders__HSTSMaxAgeDays`
- `SecurityHeaders__CSPConnectSources` (comma-separated)

## Sentry Error Tracking

```json
{
  "Sentry": {
    "Dsn": "",
    "Environment": "development",
    "TracesSampleRate": 1.0,
    "ProfilesSampleRate": 1.0,
    "EnableTracing": true,
    "SendDefaultPii": false,
    "AttachStacktrace": true
  }
}
```

**Environment Variable:** `SENTRY_DSN` (takes precedence over config file)

Leave `Dsn` empty to disable Sentry.

## File Storage

```json
{
  "FileStorage": {
    "UploadDirectory": "uploads",
    "MaxFileSizeMB": 10,
    "MaxTotalSizeMB": 50
  }
}
```

**Environment Variables:**
- `FileStorage__UploadDirectory`
- `FileStorage__MaxFileSizeMB`
- `FileStorage__MaxTotalSizeMB`

## CORS Configuration

```json
{
  "AllowedOrigins": [
    "http://localhost:3000",
    "http://localhost:3001",
    "http://localhost:5173",
    "http://localhost:8080"
  ]
}
```

**Environment Variable:** `AllowedOrigins` (comma-separated)

## Frontend Configuration

```json
{
  "Frontend": {
    "BaseUrl": "http://localhost:3001"
  }
}
```

Used for generating email links.

**Environment Variable:** `Frontend__BaseUrl`

## Super Admin Configuration

```json
{
  "SuperAdmin": {
    "Email": "superadmin@intellipm.com",
    "Password": "Super@dmin123456",
    "Username": "superadmin",
    "FirstName": "Super",
    "LastName": "Admin"
  }
}
```

**⚠️ WARNING:** Change these values in production!

**Environment Variables:**
- `SuperAdmin__Email`
- `SuperAdmin__Password`
- `SuperAdmin__Username`
- `SuperAdmin__FirstName`
- `SuperAdmin__LastName`

## Environment Variables

ASP.NET Core supports environment variables using double underscores (`__`) as separators for nested configuration.

### Example: Setting SMTP Host
```bash
# Linux/Mac
export Email__SmtpHost=smtp.example.com

# Windows PowerShell
$env:Email__SmtpHost="smtp.example.com"

# Windows CMD
set Email__SmtpHost=smtp.example.com
```

### Docker Environment Variables
```yaml
environment:
  - Email__SmtpHost=smtp.example.com
  - Email__SmtpPort=587
  - Email__SmtpUsername=user@example.com
  - Email__SmtpPassword=your-password
  - SENTRY_DSN=https://your-sentry-dsn@sentry.io/project-id
```

## Health Checks

Health check endpoints are available at:
- `/api/v1/health` - Overall health (includes SMTP, database, Ollama, memory)
- `/api/health` - Overall health (legacy endpoint)
- `/api/health/ready` - Readiness check (database only)
- `/api/health/live` - Liveness check (app is running)

Health checks UI is available at `/health-ui` (if enabled).

## Production Checklist

Before deploying to production:

- [ ] Change `Jwt:SecretKey` to a secure random value (at least 32 characters)
- [ ] Configure SMTP settings (`Email:SmtpHost`, `Email:SmtpUsername`, `Email:SmtpPassword`)
- [ ] Update `SuperAdmin` credentials
- [ ] Set `Sentry:Dsn` if using error tracking
- [ ] Configure `AllowedOrigins` for your frontend domain(s)
- [ ] Set `SecurityHeaders:EnableHSTS` to `true` if using HTTPS
- [ ] Update `Frontend:BaseUrl` to production URL
- [ ] Review and adjust rate limiting settings
- [ ] Ensure connection strings point to production databases
- [ ] Set `DevSeed:Enabled` to `false` if present
- [ ] Review `Serilog` logging configuration
- [ ] Verify health checks are accessible

