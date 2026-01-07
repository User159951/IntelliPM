# IntelliPM Permission Flow Architecture

## Permission Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         User Request                            │
│                    (UI Action or API Call)                      │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Frontend Permission Check                     │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  usePermissions() hook or PermissionGuard component      │  │
│  │  - Checks global role (Admin/User/SuperAdmin)            │  │
│  │  - Checks project role (ProductOwner/ScrumMaster/etc.)    │  │
│  │  - Returns permission status for UX                      │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
                    ┌────────────────┐
                    │  Has Permission? │
                    └────────┬─────────┘
                             │
                ┌────────────┴────────────┐
                │                         │
              YES                        NO
                │                         │
                ▼                         ▼
    ┌──────────────────────┐   ┌──────────────────────┐
    │  Show UI / Allow     │   │  Hide UI / Block     │
    │  API Call            │   │  Show Error          │
    └──────────┬───────────┘   └──────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────────────────────┐
│                      API Request Sent                           │
│              (with JWT token in HTTP-only cookie)               │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Backend Authentication                       │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  JWT Token Validation                                     │  │
│  │  - Extract user ID and organization ID                    │  │
│  │  - Verify token signature and expiration                 │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Backend Permission Check                     │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  [RequirePermission] attribute on controller            │  │
│  │  - Checks global permissions (RolePermissions table)     │  │
│  │  - Checks project role (ProjectMember table)            │  │
│  │  - Validates workflow rules (WorkflowTransitionRules)    │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
                    ┌────────────────┐
                    │  Has Permission? │
                    └────────┬─────────┘
                             │
                ┌────────────┴────────────┐
                │                         │
              YES                        NO
                │                         │
                ▼                         ▼
    ┌──────────────────────┐   ┌──────────────────────┐
    │  Execute Handler    │   │  Return 403 Forbidden│
    │  - Command Handler   │   │  with error message  │
    │  - Query Handler    │   └──────────────────────┘
    │  - Domain Logic      │
    └──────────┬───────────┘
               │
               ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Domain Validation                            │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Additional Role Checks in Domain Methods                 │  │
│  │  - Workflow transition validation                       │  │
│  │  - Entity-level permission checks                        │  │
│  │  - Business rule enforcement                             │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
                    ┌────────────────┐
                    │  Valid?        │
                    └────────┬────────┘
                             │
                ┌────────────┴────────────┐
                │                         │
              YES                        NO
                │                         │
                ▼                         ▼
    ┌──────────────────────┐   ┌──────────────────────┐
    │  Execute Operation   │   │  Return Validation   │
    │  - Update Database   │   │  Error               │
    │  - Publish Events    │   └──────────────────────┘
    │  - Return Success    │
    └──────────┬───────────┘
               │
               ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Response Sent                              │
│              (Success or Error with Details)                    │
└─────────────────────────────────────────────────────────────────┘
```

## Permission Sources

### Frontend Permission Sources

1. **AuthContext** (`src/contexts/AuthContext.tsx`)
   - User global role (Admin/User/SuperAdmin)
   - User permissions array from backend

2. **useUserRole Hook** (`src/hooks/useUserRole.ts`)
   - Fetches project role from API
   - Provides role-based flags (isScrumMaster, isProductOwner, etc.)

3. **usePermissions Hook** (`src/hooks/usePermissions.ts`)
   - Maps roles to permissions
   - Combines global and project permissions
   - Provides permission checking methods

### Backend Permission Sources

1. **RolePermissions Table**
   - Maps global roles to permissions
   - Used for organization-wide permissions

2. **ProjectMember Table**
   - Stores user's role in each project
   - Used for project-specific permissions

3. **WorkflowTransitionRules Table**
   - Defines allowed status transitions per role
   - Enforces workflow rules

4. **AIDecisionApprovalPolicy Table**
   - Defines who can approve AI decisions
   - Organization and decision-type specific

## Permission Enforcement Layers

### Layer 1: Frontend UI (UX Only)
- **Purpose**: Improve user experience by hiding unavailable actions
- **Components**: `PermissionGuard`, `usePermissions` hook
- **Note**: Can be bypassed - backend always validates

### Layer 2: API Controller
- **Purpose**: First line of backend defense
- **Mechanism**: `[RequirePermission]` attribute
- **Validation**: Checks role permissions before handler execution

### Layer 3: Command/Query Handler
- **Purpose**: Business logic validation
- **Mechanism**: Explicit role checks in handlers
- **Validation**: Validates user role and project membership

### Layer 4: Domain Logic
- **Purpose**: Final validation at domain level
- **Mechanism**: Domain methods check permissions
- **Validation**: Workflow rules, entity-level checks

## Role Hierarchy

```
Global Roles (Organization-wide)
├── SuperAdmin (All organizations, all permissions)
├── Admin (Own organization, all permissions)
└── User (Basic permissions)

Project Roles (Project-specific)
├── ProductOwner (Full project control)
│   └── Inherits ScrumMaster permissions
├── ScrumMaster (Sprint management)
│   └── Exclusive: Start/Close sprints
├── Developer (Task management)
├── Tester/QA (Quality assurance)
│   └── Exclusive: Approve releases
├── Manager (Stakeholder visibility)
└── Viewer (Read-only)
```

## Permission Flow Examples

### Example 1: Starting a Sprint

```
1. User clicks "Start Sprint" button
   ↓
2. Frontend: PermissionGuard checks if user is ScrumMaster
   ↓
3. If yes: Button enabled, API call made
   ↓
4. Backend: [RequirePermission("sprints.start")] validates
   ↓
5. Handler: Checks user role is ScrumMaster
   ↓
6. Domain: Validates sprint can be started (has tasks, dates, etc.)
   ↓
7. Success: Sprint status updated to "Active"
```

### Example 2: Approving a Release

```
1. User clicks "Approve Release" button
   ↓
2. Frontend: PermissionGuard checks if user is Tester
   ↓
3. If yes: Button enabled, API call made
   ↓
4. Backend: [RequirePermission("releases.approve")] validates
   ↓
5. Handler: Checks user role is Tester
   ↓
6. Domain: Validates quality gates are passed
   ↓
7. Success: Release status updated to "ReadyForDeployment"
```

### Example 3: Permission Denied

```
1. User tries to delete a task (Developer role)
   ↓
2. Frontend: PermissionGuard hides delete button (UX)
   ↓
3. User somehow triggers delete (e.g., direct API call)
   ↓
4. Backend: [RequirePermission("tasks.delete")] checks
   ↓
5. Validation fails: Developer doesn't have delete permission
   ↓
6. Response: 403 Forbidden with error message
```

## Multi-Tenancy Isolation

```
Organization A                    Organization B
├── User 1 (Admin)               ├── User 3 (Admin)
├── User 2 (User)                 ├── User 4 (User)
└── Projects                      └── Projects
    ├── Project 1                    ├── Project 3
    │   └── Members                   │   └── Members
    │       ├── User 1 (ProductOwner) │       ├── User 3 (ProductOwner)
    │       └── User 2 (Developer)    │       └── User 4 (Developer)
    └── Project 2                    └── Project 4

Permission Isolation:
- User 1 can only access Organization A data
- User 3 can only access Organization B data
- SuperAdmin can access all organizations
```

## Key Components

### Frontend Components

- **PermissionGuard**: React component that conditionally renders children based on permissions
- **usePermissions**: Hook for checking permissions
- **useUserRole**: Hook for checking user roles
- **usePermissionsWithProject**: Hook for project-specific permissions

### Backend Components

- **PermissionAuthorizationHandler**: ASP.NET Core authorization handler
- **PermissionPolicyProvider**: Policy provider for permission-based authorization
- **RequirePermissionAttribute**: Attribute for protecting endpoints
- **WorkflowValidator**: Validates workflow transitions

## Best Practices

1. **Always validate on backend**: Frontend checks are for UX only
2. **Use explicit permissions**: Don't rely solely on roles
3. **Check at multiple layers**: API, handler, and domain
4. **Log permission denials**: For security auditing
5. **Provide clear error messages**: Help users understand why access was denied

---

*Last Updated: 2025-01-06*
*Version: 1.0*

