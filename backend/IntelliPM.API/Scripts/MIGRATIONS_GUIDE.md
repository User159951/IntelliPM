# Database Migrations Guide

This guide explains how to work with the dual-database architecture in IntelliPM.

## Architecture Overview

IntelliPM uses two separate databases:

1. **SQL Server** (`AppDbContext`) - Transactional data
   - Users, Projects, Backlogs, Sprints, Risks, KPIs, Defects, Insights, Alerts
   - Connection string: `ConnectionStrings:SqlServer`

2. **PostgreSQL + pgvector** (`VectorDbContext`) - Agent memory and embeddings
   - Agent memory records with vector embeddings
   - Connection string: `ConnectionStrings:VectorDb`

## Prerequisites

- .NET 8 SDK
- SQL Server (or Docker container)
- PostgreSQL 15+ with pgvector extension (or Docker container)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

## Initial Setup

### Step 1: Initialize PostgreSQL with pgvector

Run the initialization script to enable the pgvector extension:

```bash
# If using Docker Compose (recommended)
docker-compose up -d postgres

# Wait for PostgreSQL to start, then initialize
docker exec -i intellipm-postgres-1 psql -U postgres -d intellipm_vector < backend/IntelliPM.API/Scripts/pgvector-init.sql

# Or if using local PostgreSQL
psql -U postgres -d intellipm_vector -f backend/IntelliPM.API/Scripts/pgvector-init.sql
```

Alternatively, connect to PostgreSQL and run:

```sql
CREATE EXTENSION IF NOT EXISTS vector;
```

### Step 2: Create Initial Migrations

Navigate to the solution directory:

```bash
cd backend
```

#### SQL Server Migrations (AppDbContext)

```bash
dotnet ef migrations add InitialCreate \
  --context AppDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API \
  --output-dir Persistence/Migrations
```

#### PostgreSQL Migrations (VectorDbContext)

```bash
dotnet ef migrations add InitialVectorDb \
  --context VectorDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API \
  --output-dir VectorStore/Migrations
```

### Step 3: Apply Migrations

#### SQL Server

```bash
dotnet ef database update \
  --context AppDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API
```

#### PostgreSQL

```bash
dotnet ef database update \
  --context VectorDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API
```

### Step 4: Run the Application

```bash
cd IntelliPM.API
dotnet run
```

The application will automatically:
- Apply pending migrations on both databases
- Seed initial data if databases are empty

## Adding New Migrations

### For Transactional Data (SQL Server)

When you add/modify entities in the Domain layer that should be stored in SQL Server:

```bash
dotnet ef migrations add <MigrationName> \
  --context AppDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API \
  --output-dir Persistence/Migrations
```

### For Vector Data (PostgreSQL)

When you modify the `AgentMemoryRecord` or add new vector-related entities:

```bash
dotnet ef migrations add <MigrationName> \
  --context VectorDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API \
  --output-dir VectorStore/Migrations
```

## Connection Strings

### Development (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost,1433;Database=IntelliPM;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;",
    "VectorDb": "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=intellipm_vector;"
  }
}
```

### Docker Compose (appsettings.json)

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=sqlserver;Database=IntelliPM;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;",
    "VectorDb": "Host=postgres;Port=5432;Username=postgres;Password=postgres;Database=intellipm_vector;"
  }
}
```

## Verification

### Check SQL Server Database

```bash
# Using Docker
docker exec -it intellipm-sqlserver-1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'YourPassword123!'

# In sqlcmd
SELECT name FROM sys.databases;
GO
USE IntelliPM;
GO
SELECT * FROM Users;
GO
```

### Check PostgreSQL Database

```bash
# Using Docker
docker exec -it intellipm-postgres-1 psql -U postgres -d intellipm_vector

# In psql
\dt                          -- List tables
\d agent_memories            -- Describe table
SELECT * FROM agent_memories LIMIT 5;
```

### Verify pgvector Extension

```sql
SELECT * FROM pg_extension WHERE extname = 'vector';
SELECT extversion FROM pg_extension WHERE extname = 'vector';
```

## Testing Vector Search

After the application is running, you can test the vector memory store:

```bash
# Using the IVectorMemoryStore interface (recommended)
curl -X POST "http://localhost:5000/api/projects/1/agents/store-note" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "decision",
    "content": "Decided to use microservices architecture for scalability"
  }'

# Search will use vector similarity via pgvector
curl -X GET "http://localhost:5000/api/projects/1/agents/search?query=architecture" \
  -H "Authorization: Bearer <your-jwt-token>"
```

## Troubleshooting

### pgvector Extension Not Found

If you get "extension vector does not exist":

```bash
# Connect to PostgreSQL
docker exec -it intellipm-postgres-1 psql -U postgres

# Run as superuser
CREATE EXTENSION IF NOT EXISTS vector;
```

### Migration Already Applied

If you get "migration already applied" errors:

```bash
# Remove the last migration
dotnet ef migrations remove \
  --context AppDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API

# Or for VectorDbContext
dotnet ef migrations remove \
  --context VectorDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API
```

### Connection String Issues

Verify your connection strings in appsettings:

```bash
dotnet run --project IntelliPM.API --urls="http://localhost:5000"
```

Check the logs for connection errors.

### Port Conflicts

If SQL Server port 1433 or PostgreSQL port 5432 is in use:

```bash
# Check what's using the ports
netstat -ano | findstr :1433
netstat -ano | findstr :5432

# Change ports in docker-compose.yml or use different local instances
```

## Production Considerations

1. **Connection String Security**: Use Azure Key Vault, AWS Secrets Manager, or environment variables
2. **Migration Strategy**: Use CI/CD pipelines to apply migrations
3. **Vector Index Optimization**: Create IVFFlat indexes for large datasets:
   ```sql
   CREATE INDEX idx_agent_memories_embedding_ivfflat 
     ON agent_memories 
     USING ivfflat (embedding vector_cosine_ops) 
     WITH (lists = 100);
   ```
4. **Backup Strategy**: Separate backup policies for transactional vs. vector data
5. **Connection Pooling**: Already configured via EF Core's connection pool settings

## Quick Commands Reference

```bash
# Create migration (SQL Server)
dotnet ef migrations add <Name> --context AppDbContext --project IntelliPM.Infrastructure --startup-project IntelliPM.API

# Create migration (PostgreSQL)
dotnet ef migrations add <Name> --context VectorDbContext --project IntelliPM.Infrastructure --startup-project IntelliPM.API

# Update database (SQL Server)
dotnet ef database update --context AppDbContext --project IntelliPM.Infrastructure --startup-project IntelliPM.API

# Update database (PostgreSQL)
dotnet ef database update --context VectorDbContext --project IntelliPM.Infrastructure --startup-project IntelliPM.API

# Generate SQL script (SQL Server)
dotnet ef migrations script --context AppDbContext --project IntelliPM.Infrastructure --startup-project IntelliPM.API

# Generate SQL script (PostgreSQL)
dotnet ef migrations script --context VectorDbContext --project IntelliPM.Infrastructure --startup-project IntelliPM.API

# List migrations
dotnet ef migrations list --context AppDbContext --project IntelliPM.Infrastructure --startup-project IntelliPM.API
```

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                      IntelliPM.API                          │
│                      (Program.cs)                           │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      │ AddInfrastructure()
                      ▼
┌─────────────────────────────────────────────────────────────┐
│              IntelliPM.Infrastructure                       │
│              (DependencyInjection.cs)                       │
├─────────────────────────────────────────────────────────────┤
│  ┌───────────────────┐        ┌──────────────────────┐    │
│  │   AppDbContext    │        │  VectorDbContext     │    │
│  │  (SQL Server)     │        │  (PostgreSQL)        │    │
│  │                   │        │  + pgvector          │    │
│  └────────┬──────────┘        └──────────┬───────────┘    │
│           │                               │                 │
│           ▼                               ▼                 │
│  ConnectionStrings:              ConnectionStrings:        │
│  SqlServer                       VectorDb                  │
└─────────────────────────────────────────────────────────────┘
           │                               │
           ▼                               ▼
  ┌──────────────────┐          ┌──────────────────────┐
  │   SQL Server     │          │   PostgreSQL         │
  │   Port: 1433     │          │   Port: 5432         │
  │                  │          │   + pgvector ext     │
  └──────────────────┘          └──────────────────────┘
```

