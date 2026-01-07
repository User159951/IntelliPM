# How to Run the OrganizationId Migration

## Prerequisites

1. Ensure you have the EF Core tools installed:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. Verify your connection string is configured in `appsettings.Development.json` or `appsettings.json`

## Option 1: Regenerate Migration (Recommended)

Since the migration file was created manually, it's better to regenerate it using EF Core tools to ensure it's properly tracked:

### Step 1: Delete the Manual Migration File

Delete this file:
```
backend/IntelliPM.Infrastructure/Persistence/Migrations/20260108000000_AddOrganizationIdToAgentExecutionLog.cs
```

### Step 2: Generate Migration Using EF Core

Navigate to the backend directory:
```bash
cd backend
```

Generate the migration:
```bash
dotnet ef migrations add AddOrganizationIdToAgentExecutionLog \
  --context AppDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API \
  --output-dir Persistence/Migrations
```

This will:
- Detect the changes to `AgentExecutionLog` entity (OrganizationId field)
- Generate the migration file automatically
- Update the `AppDbContextModelSnapshot.cs` to track the changes

### Step 3: Apply the Migration

Apply the migration to your database:
```bash
dotnet ef database update \
  --context AppDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API
```

---

## Option 2: Apply Manual Migration Directly

If you want to use the manually created migration file:

### Step 1: Verify Migration File Exists

Ensure the migration file exists at:
```
backend/IntelliPM.Infrastructure/Persistence/Migrations/20260108000000_AddOrganizationIdToAgentExecutionLog.cs
```

### Step 2: Update the Model Snapshot

You may need to manually update `AppDbContextModelSnapshot.cs` to include the OrganizationId field. However, **this is not recommended** as it can cause issues.

### Step 3: Apply the Migration

```bash
cd backend

dotnet ef database update \
  --context AppDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API
```

**Note:** EF Core might complain that the migration is not in the snapshot. If this happens, use Option 1 instead.

---

## Verify Migration Applied

### Check Migration Status

```bash
dotnet ef migrations list \
  --context AppDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API
```

You should see `AddOrganizationIdToAgentExecutionLog` in the list.

### Verify Database Schema

Connect to your SQL Server database and verify:

```sql
-- Check if OrganizationId column exists
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AgentExecutionLogs'
AND COLUMN_NAME = 'OrganizationId';

-- Check if foreign key exists
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTableName,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumnName
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fc
    ON fk.object_id = fc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'AgentExecutionLogs'
AND COL_NAME(fc.parent_object_id, fc.parent_column_id) = 'OrganizationId';
```

---

## Important: Update Existing Records

After applying the migration, existing `AgentExecutionLog` records will have `OrganizationId = 0`. You should update them based on the `UserId` field:

```sql
-- Update existing records based on User's OrganizationId
UPDATE ael
SET ael.OrganizationId = u.OrganizationId
FROM AgentExecutionLogs ael
INNER JOIN Users u ON CAST(ael.UserId AS INT) = u.Id
WHERE ael.OrganizationId = 0
AND u.OrganizationId IS NOT NULL;
```

**Note:** This assumes `UserId` in `AgentExecutionLog` is stored as a string that can be cast to INT. Adjust the query if your `UserId` format is different.

---

## Troubleshooting

### Error: "Migration not found in snapshot"

**Solution:** Use Option 1 to regenerate the migration using EF Core tools.

### Error: "Cannot add foreign key constraint"

**Possible causes:**
- The `Organizations` table doesn't exist
- There are existing records with invalid OrganizationId values

**Solution:**
1. Verify the Organizations table exists
2. Update existing records first (see SQL above)
3. Then apply the migration

### Error: "Column already exists"

**Solution:** The migration was already applied. Check migration status:
```bash
dotnet ef migrations list --context AppDbContext --project IntelliPM.Infrastructure --startup-project IntelliPM.API
```

---

## Quick Reference

```bash
# Navigate to backend directory
cd backend

# Generate migration (if regenerating)
dotnet ef migrations add AddOrganizationIdToAgentExecutionLog \
  --context AppDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API \
  --output-dir Persistence/Migrations

# Apply migration
dotnet ef database update \
  --context AppDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API

# Check migration status
dotnet ef migrations list \
  --context AppDbContext \
  --project IntelliPM.Infrastructure \
  --startup-project IntelliPM.API
```

