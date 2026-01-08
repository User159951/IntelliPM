# Generated Types

This directory contains TypeScript types automatically generated from the backend OpenAPI/Swagger specification.

## Files

- `api.ts` - Complete OpenAPI type definitions generated from the backend API spec
- `enums.ts` - Extracted enum types and type aliases for easier use throughout the application

## Regenerating Types

To regenerate types from the backend API:

```bash
npm run generate:types
```

This command:
1. Fetches the OpenAPI spec from the backend Swagger endpoint
2. Generates TypeScript types using `openapi-typescript`
3. Outputs the types to `api.ts`
4. Generates `enums.ts` with extracted enum types including:
   - Status types (ProjectStatus, SprintStatus, TaskStatus, DefectStatus, etc.)
   - Role types (GlobalRole, ProjectRole)
   - Notification types (NotificationType, NotificationFrequency)
   - AI Decision types (AIDecisionType, AIAgentType, AIDecisionStatus)
   - Release, Milestone, Quality Gate, and Dependency types

**Note:** The backend API must be running for this command to work. The default endpoint is `http://localhost:5001/swagger/v1/swagger.json`, but you can override it with the `VITE_API_BASE_URL` environment variable.

## Pre-commit Hook

A pre-commit hook is configured to automatically regenerate types before each commit. This ensures that types are always in sync with the backend API. The hook:
1. Runs `npm run generate:types`
2. Stages the generated `api.ts` and `enums.ts` files

## Usage

Import types from the generated files:

```typescript
// Import enum types
import { 
  ProjectStatus, 
  TaskStatus, 
  GlobalRole,
  NotificationType,
  AIDecisionType,
  AIAgentType,
  AIDecisionStatus
} from '@/types/generated/enums';

// Import full API types
import type { components } from '@/types/generated/api';
type ProjectDto = components['schemas']['IntelliPM.Application.Projects.Queries.ProjectListDto'];
```

## Important Notes

- **DO NOT** manually edit files in this directory
- Types are automatically generated and will be overwritten
- If you need to add custom types, add them to `src/types/` (not in `generated/`)
- The generated types use string literal unions for enums (since the backend serializes enums as strings)
- All business logic enums (Status, Roles, NotificationTypes, AI Decision Types) are now generated from the backend OpenAPI spec

