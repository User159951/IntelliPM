# Role-Based Access Control (RBAC) Documentation

## Overview

IntelliPM uses a **pure role-based authorization model** for Admin and SuperAdmin roles. This document defines the standardized approach for authorization across the application.

## Decision: Pure Role-Based Authorization

**Selected Approach**: Option A - Pure Role-Based (IsAdmin, IsSuperAdmin checks)

### Rationale

1. **Clear Hierarchy**: Admin and SuperAdmin are global roles with a well-defined hierarchy
2. **Organization Scoping**: The distinction between Admin (own organization) and SuperAdmin (all organizations) is fundamental and role-based
3. **Simplicity**: Role checks are simpler and more performant than permission checks for these global roles
4. **Consistency**: Most of the codebase already uses role-based checks for Admin/SuperAdmin
5. **Maintainability**: Easier for developers to understand and maintain

### When to Use Permission-Based Authorization

- **Project-level permissions**: Use `[RequirePermission]` for project-specific actions (e.g., `projects.create`, `tasks.edit`)
- **Feature flags**: Use `[RequireFeatureFlag]` for feature-gated functionality
- **Granular access control**: When you need fine-grained permissions beyond role boundaries

## Role Hierarchy

```
SuperAdmin (Highest)
    ↓
Admin
    ↓
User (Lowest)
```

### Role Definitions

#### User
- **Scope**: Standard user with basic permissions
- **Organization Access**: Own organization only
- **Capabilities**:
  - View projects they're members of
  - Create new projects (unless restricted)
  - Access basic features based on project role

#### Admin
- **Scope**: Organization administrator
- **Organization Access**: **Own organization only**
- **Capabilities**:
  - All User permissions
  - Manage organization settings
  - Manage users within their organization
  - View admin panel
  - Configure AI quotas and governance for their organization
- **Limitations**: 
  - Cannot access other organizations' data
  - Cannot perform system-wide operations

#### SuperAdmin
- **Scope**: System administrator
- **Organization Access**: **All organizations**
- **Capabilities**:
  - All Admin permissions
  - Access all organizations
  - Manage system-wide settings
  - Approve AI decisions from any organization
  - System health monitoring
  - Manage organization-level configurations
- **Limitations**: None (full system access)

## Authorization Attributes

### Backend Attributes

#### `[RequireAdmin]`
- **Purpose**: Requires Admin or SuperAdmin role
- **Usage**: `[RequireAdmin]` on controller class or action method
- **Access**: Admin (own org) and SuperAdmin (all orgs)
- **Example**:
```csharp
[RequireAdmin]
public class OrganizationController : BaseApiController
{
    // Admin can access their own org, SuperAdmin can access all orgs
}
```

#### `[RequireSuperAdmin]`
- **Purpose**: Requires SuperAdmin role only
- **Usage**: `[RequireSuperAdmin]` on controller class or action method
- **Access**: SuperAdmin only
- **Example**:
```csharp
[RequireSuperAdmin]
public class OrganizationsController : BaseApiController
{
    // Only SuperAdmin can access
}
```

### Frontend Guards

#### `RequireAdminGuard`
- **Purpose**: Route guard for Admin/SuperAdmin routes
- **Usage**: Wrap admin routes/components
- **Access**: Admin (own org) and SuperAdmin (all orgs)
- **Example**:
```tsx
<Route
  path="/admin"
  element={
    <RequireAdminGuard>
      <AdminLayout />
    </RequireAdminGuard>
  }
/>
```

#### `RequireSuperAdminGuard`
- **Purpose**: Route guard for SuperAdmin-only routes
- **Usage**: Wrap SuperAdmin routes/components
- **Access**: SuperAdmin only
- **Example**:
```tsx
<Route
  path="/admin/organizations"
  element={
    <RequireSuperAdminGuard>
      <AdminOrganizations />
    </RequireSuperAdminGuard>
  }
/>
```

## Organization Scoping

### Admin Organization Access
- Admin users can **only** access resources from their own organization
- Organization scoping is enforced via `OrganizationScopingService`
- Attempts to access other organizations result in `UnauthorizedException`

### SuperAdmin Organization Access
- SuperAdmin users can access **all** organizations
- No organization scoping restrictions
- Can manage system-wide configurations

### Implementation

The `OrganizationScopingService` handles organization scoping:

```csharp
// Admin: Returns user's OrganizationId
// SuperAdmin: Returns 0 (all organizations)
int GetScopedOrganizationId();

// Admin: Throws if accessing different org
// SuperAdmin: Always allows
void EnsureOrganizationAccess(int requestedOrganizationId);
```

## Authorization Patterns

### Controller-Level Authorization

```csharp
// Admin endpoints (Admin + SuperAdmin)
[RequireAdmin]
[Route("api/admin/organization")]
public class OrganizationController : BaseApiController
{
    // Admin can access own org, SuperAdmin can access all
}

// SuperAdmin-only endpoints
[RequireSuperAdmin]
[Route("api/admin/organizations")]
public class OrganizationsController : BaseApiController
{
    // Only SuperAdmin can access
}
```

### Action-Level Authorization

```csharp
[RequireAdmin]
public class UsersController : BaseApiController
{
    // Admin + SuperAdmin can invite
    [HttpPost("invite")]
    public async Task<IActionResult> InviteUser(...) { }
    
    // SuperAdmin-only action
    [RequireSuperAdmin]
    [HttpPost("system-reset")]
    public async Task<IActionResult> SystemReset(...) { }
}
```

### Frontend Route Protection

```tsx
// Admin routes
<Route
  path="/admin/*"
  element={
    <RequireAdminGuard>
      <AdminLayout />
    </RequireAdminGuard>
  }
/>

// SuperAdmin-only routes
<Route
  path="/admin/organizations"
  element={
    <RequireSuperAdminGuard>
      <AdminOrganizations />
    </RequireSuperAdminGuard>
  }
/>
```

### Frontend Component-Level Checks

```tsx
const { isAdmin, isSuperAdmin } = useAuth();

// Show admin features
{isAdmin && <AdminPanel />}

// Show SuperAdmin-only features
{isSuperAdmin && <SystemSettings />}
```

## Migration Guide

### Replacing `[Authorize(Roles = "Admin,SuperAdmin")]`

**Before**:
```csharp
[Authorize(Roles = "Admin,SuperAdmin")]
public class OrganizationController : BaseApiController
```

**After**:
```csharp
[RequireAdmin]
public class OrganizationController : BaseApiController
```

### Replacing `[Authorize(Roles = "SuperAdmin")]`

**Before**:
```csharp
[Authorize(Roles = "SuperAdmin")]
public class OrganizationsController : BaseApiController
```

**After**:
```csharp
[RequireSuperAdmin]
public class OrganizationsController : BaseApiController
```

## Testing Requirements

### Integration Tests

1. **Admin Access Tests**:
   - Admin can access own organization resources
   - Admin cannot access other organizations' resources
   - Admin receives 403 Forbidden when attempting cross-org access

2. **SuperAdmin Access Tests**:
   - SuperAdmin can access all organizations
   - SuperAdmin can perform system-wide operations
   - SuperAdmin bypasses organization scoping

3. **Cross-Organization Access Prevention**:
   - Verify `OrganizationScopingService.EnsureOrganizationAccess()` throws for Admin
   - Verify `OrganizationScopingService.EnsureOrganizationAccess()` allows for SuperAdmin

### Frontend Tests

1. **Route Guard Tests**:
   - `RequireAdminGuard` redirects non-admin users
   - `RequireSuperAdminGuard` redirects non-superadmin users
   - Guards allow access for appropriate roles

2. **UI Component Tests**:
   - Admin-only UI elements hidden for regular users
   - SuperAdmin-only UI elements hidden for Admin users
   - Role-based rendering works correctly

## Best Practices

1. **Use Role-Based for Global Roles**: Always use `[RequireAdmin]` or `[RequireSuperAdmin]` for Admin/SuperAdmin authorization
2. **Use Permission-Based for Project Actions**: Use `[RequirePermission]` for project-level permissions
3. **Enforce Organization Scoping**: Always use `OrganizationScopingService` for Admin users
4. **Fail Closed**: When in doubt, deny access
5. **Document Exceptions**: If you need to bypass standard patterns, document why
6. **Test Cross-Org Access**: Always test that Admin cannot access other organizations

## Common Mistakes to Avoid

1. ❌ **Mixing Approaches**: Don't use `[Authorize(Roles = "...")]` when `[RequireAdmin]` or `[RequireSuperAdmin]` should be used
2. ❌ **Missing Organization Scoping**: Don't forget to enforce organization boundaries for Admin users
3. ❌ **Frontend-Only Checks**: Don't rely solely on frontend guards; backend must enforce authorization
4. ❌ **Assuming SuperAdmin Inherits Admin**: While SuperAdmin has all Admin capabilities, use explicit `[RequireSuperAdmin]` for SuperAdmin-only endpoints

## Related Documentation

- [RolesAndPermissions.md](./RolesAndPermissions.md) - Detailed role and permission definitions
- [PermissionMatrix.md](./PermissionMatrix.md) - Permission matrix for project-level permissions
- [AIGovernanceGuide.md](./AIGovernanceGuide.md) - AI governance and approval workflows
