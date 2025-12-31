# Health Checks

IntelliPM provides multiple health check endpoints for monitoring application health and API availability.

## Available Endpoints

### 1. General Health Check

**Endpoint:** `GET /api/health`
**Authentication:** None (public)
**Description:** Checks overall application health including database, Ollama AI service, and memory usage.

**Response Example:**

```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "ollama": "Healthy",
    "memory": "Healthy"
  },
  "timestamp": "2025-12-31T14:00:00Z"
}
```

**Health Checks Performed:**

- **Database** (SQL Server): Verifies database connectivity
- **Ollama** (AI Service): Checks Ollama LLM availability
- **Memory**: Monitors memory usage thresholds

### 2. API Smoke Tests

**Endpoint:** `GET /api/health/api`
**Authentication:** None (public)
**Description:** Performs smoke tests on critical API endpoints to verify routing and authentication.

**Response Example:**

```json
{
  "status": "Healthy",
  "checks": [
    {
      "endpoint": "/api/v1/Auth/me",
      "description": "Authentication endpoint routing",
      "status": "OK",
      "expectedStatus": 401,
      "actualStatus": 401,
      "responseTime": 12
    },
    {
      "endpoint": "/api/v1/Projects",
      "description": "Projects endpoint routing",
      "status": "OK",
      "expectedStatus": 401,
      "actualStatus": 401,
      "responseTime": 15
    },
    {
      "endpoint": "/api/health",
      "description": "Public health endpoint",
      "status": "OK",
      "expectedStatus": 200,
      "actualStatus": 200,
      "responseTime": 8
    },
    {
      "endpoint": "/swagger/index.html",
      "description": "Swagger documentation",
      "status": "OK",
      "expectedStatus": 200,
      "actualStatus": 200,
      "responseTime": 10
    }
  ],
  "timestamp": "2025-12-31T14:00:00Z"
}
```

**Status Values:**

- `Healthy`: All checks passed
- `Degraded`: Some checks failed but application is functional
- `Unhealthy`: Critical checks failed

**Endpoints Tested:**

- `/api/v1/Auth/me` - Authentication endpoint (expected: 401 Unauthorized)
- `/api/v1/Projects` - Projects endpoint (expected: 401 Unauthorized)
- `/api/health` - Public health endpoint (expected: 200 OK)
- `/swagger/index.html` - Swagger documentation (expected: 200 OK)

## Usage

### Development

```bash
curl http://localhost:5001/api/health
curl http://localhost:5001/api/health/api
```

### Production

```bash
curl https://api.intellipm.com/api/health
curl https://api.intellipm.com/api/health/api
```

### Docker Health Check

Add to `docker-compose.yml`:

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:5001/api/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

### Kubernetes Liveness Probe

```yaml
livenessProbe:
  httpGet:
    path: /api/health
    port: 5001
  initialDelaySeconds: 30
  periodSeconds: 10
```

### Kubernetes Readiness Probe

```yaml
readinessProbe:
  httpGet:
    path: /api/health/api
    port: 5001
  initialDelaySeconds: 10
  periodSeconds: 5
```

## Monitoring Integration

### Prometheus

Expose health check metrics for Prometheus scraping.

### Grafana

Create dashboards using health check endpoints as data sources.

### Azure Application Insights

Health checks automatically logged via Sentry integration.

## Troubleshooting

### Database Unhealthy

- Check connection string in `appsettings.json`
- Verify SQL Server is running
- Check network connectivity

### Ollama Unhealthy

- Verify Ollama service is running
- Check Ollama URL in configuration
- Ensure model is downloaded

### Memory Issues

- Check container memory limits
- Review memory usage patterns
- Consider scaling horizontally

### API Smoke Tests Failing

- Verify all endpoints are accessible
- Check routing configuration
- Verify authentication middleware is working correctly
- Check BaseUrl configuration in `appsettings.json`

## Configuration

### BaseUrl

Configure the base URL for smoke tests in `appsettings.json`:

```json
{
  "BaseUrl": "http://localhost:5001"
}
```

For production, set via environment variable:

```bash
BaseUrl=https://api.intellipm.com
```

## Implementation Details

Health checks are implemented using:

- **ASP.NET Core Health Checks**: Built-in framework
- **Custom Health Checks**: DatabaseHealthCheck, OllamaHealthCheck, MemoryHealthCheck
- **IHttpClientFactory**: For API smoke tests
- **Timeout Handling**: 5-second timeout for external calls

For implementation details, see:

- `IntelliPM.API/Controllers/HealthController.cs`
- `IntelliPM.API/Controllers/HealthApiController.cs`
- `IntelliPM.Infrastructure/HealthChecks/`

