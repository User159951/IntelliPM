# Permission Audit Report

**Date:** January 2025  
**Status:** In Progress  
**Purpose:** Document frontend-backend permission mapping and audit all action buttons/forms

---

## Table of Contents

1. [Permission System Overview](#permission-system-overview)
2. [Frontend Permission Implementation](#frontend-permission-implementation)
3. [Backend Permission Implementation](#backend-permission-implementation)
4. [Permission Mapping Matrix](#permission-mapping-matrix)
5. [Component Audit](#component-audit)
6. [Remaining Work](#remaining-work)
7. [Best Practices](#best-practices)

---

## Permission System Overview

### Frontend Permission Constants

All permissions are defined in `frontend/src/hooks/usePermissions.ts`:

```typescript
export const PERMISSIONS = {
  // User management permissions
  USERS_MANAGE: 'users.manage',
  USERS_CREATE: 'users.create',
  USERS_UPDATE: 'users.update',
  USERS_DELETE: 'users.delete',
  USERS_VIEW: 'users.view',

  // Admin permissions
  ADMIN_PANEL_VIEW: 'admin.panel.view',
  ADMIN_SETTINGS_UPDATE: 'admin.settings.update',
  ADMIN_PERMISSIONS_UPDATE: 'admin.permissions.update',

  // Project permissions
  PROJECTS_CREATE: 'projects.create',
  PROJECTS_VIEW: 'projects.view',
  PROJECTS_EDIT: 'projects.edit',
  PROJECTS_DELETE: 'projects.delete',
  PROJECTS_MEMBERS_INVITE: 'projects.members.invite',
  PROJECTS_MEMBERS_REMOVE: 'projects.members.remove',
  PROJECTS_MEMBERS_CHANGE_ROLE: 'projects.members.changeRole',

  // Task permissions
  TASKS_CREATE: 'tasks.create',
  TASKS_EDIT: 'tasks.edit',
  TASKS_DELETE: 'tasks.delete',
  TASKS_VIEW: 'tasks.view',
  TASKS_ASSIGN: 'tasks.assign',
  TASKS_COMMENT: 'tasks.comment',

  // Sprint permissions
  SPRINTS_CREATE: 'sprints.create',
  SPRINTS_EDIT: 'sprints.edit',
  SPRINTS_DELETE: 'sprints.delete',
  SPRINTS_MANAGE: 'sprints.manage',
  SPRINTS_VIEW: 'sprints.view',

  // Defect permissions
  DEFECTS_CREATE: 'defects.create',
  DEFECTS_EDIT: 'defects.edit',
  DEFECTS_DELETE: 'defects.delete',
  DEFECTS_VIEW: 'defects.view',

  // Backlog permissions
  BACKLOG_CREATE: 'backlog.create',
  BACKLOG_EDIT: 'backlog.edit',
  BACKLOG_DELETE: 'backlog.delete',
  BACKLOG_VIEW: 'backlog.view',
} as const;
```

### Backend Permission Enforcement

Backend uses `[RequirePermission("resource.action")]` attributes on controller endpoints:

```csharp
[RequirePermission("projects.create")]
public async Task<IActionResult> CreateProject(...)
```

---

## Frontend Permission Implementation

### Hooks

1. **`usePermissions()`** - Global permission checking
   - Checks global role (Admin/User)
   - Checks explicit permissions from user.permissions array
   - Admins have all permissions automatically

2. **`usePermissionsWithProject(projectId)`** - Project-specific permissions
   - Fetches user's project role
   - Combines global permissions with project role permissions
   - Use for project-scoped actions

3. **`useUserRole(projectId?)`** - Role-based UI customization
   - Returns global and project roles
   - Provides convenience flags (isAdmin, isScrumMaster, etc.)
   - Use for conditional UI rendering based on roles

### Components

1. **`PermissionGuard`** - Conditional rendering based on permissions
   ```tsx
   <PermissionGuard 
     requiredPermission="projects.create" 
     projectId={projectId}
     fallback={null}
     showNotification={false}
   >
     <CreateButton />
   </PermissionGuard>
   ```

2. **`PermissionButton`** - Button with permission check and tooltip
   ```tsx
   <PermissionButton
     hasPermission={can('projects.create')}
     permissionName="projects.create"
   >
     Create Project
   </PermissionButton>
   ```

### Error Handling

- 403 errors are automatically caught in `api/client.ts`
- Permission errors are extracted and formatted with user-friendly messages
- Toast notifications show: "You need [Permission Name] permission to perform this action"

---

## Backend Permission Implementation

### Controllers with RequirePermission

| Controller | Endpoints | Permission Pattern |
|------------|-----------|-------------------|
| `ProjectsController` | 13 | `projects.*` |
| `TasksController` | 11 | `tasks.*` |
| `SprintsController` | 8 | `sprints.*` |
| `DefectsController` | 5 | `defects.*` |
| `UsersController` | 6 | `users.*` |
| `CommentsController` | 4 | `tasks.comment` (implicit) |
| `AttachmentsController` | 4 | Context-dependent |
| `AgentsController` | 6 | `projects.view` |

---

## Permission Mapping Matrix

### Projects

| Action | Frontend Permission | Backend Permission | Status | Notes |
|--------|-------------------|-------------------|--------|-------|
| Create Project | `projects.create` | `projects.create` | ✅ Fixed | Projects.tsx - Create button guarded |
| Edit Project | `projects.edit` | `projects.edit` | ✅ Fixed | Projects.tsx - Edit menu item guarded |
| Delete Project | `projects.delete` | `projects.delete` | ✅ Fixed | Projects.tsx - Delete menu item guarded |
| View Projects | `projects.view` | `projects.view` | ✅ OK | Default permission |
| Invite Members | `projects.members.invite` | `projects.members.invite` | ⚠️ Needs Audit | Check ProjectMembers.tsx |
| Remove Members | `projects.members.remove` | `projects.members.remove` | ⚠️ Needs Audit | Check ProjectMembers.tsx |
| Change Member Role | `projects.members.changeRole` | `projects.members.changeRole` | ⚠️ Needs Audit | Check ProjectMembers.tsx |

### Tasks

| Action | Frontend Permission | Backend Permission | Status | Notes |
|--------|-------------------|-------------------|--------|-------|
| Create Task | `tasks.create` | `tasks.create` | ⚠️ Needs Audit | Check Tasks.tsx, CreateTaskDialog.tsx |
| Edit Task | `tasks.edit` | `tasks.edit` | ⚠️ Needs Audit | Check TaskDetailSheet.tsx |
| Delete Task | `tasks.delete` | `tasks.delete` | ⚠️ Needs Audit | Check TaskDetailSheet.tsx, TaskListView.tsx |
| View Tasks | `tasks.view` | `tasks.view` | ✅ OK | Default permission |
| Assign Task | `tasks.assign` | `tasks.assign` | ⚠️ Needs Audit | Check TaskDetailSheet.tsx |
| Comment on Task | `tasks.comment` | `tasks.comment` | ⚠️ Needs Audit | Check CommentForm.tsx |

### Sprints

| Action | Frontend Permission | Backend Permission | Status | Notes |
|--------|-------------------|-------------------|--------|-------|
| Create Sprint | `sprints.create` | `sprints.create` | ✅ Fixed | Sprints.tsx - Create button guarded |
| Edit Sprint | `sprints.edit` | `sprints.edit` | ⚠️ Needs Audit | Check Sprints.tsx, EditSprintDialog.tsx |
| Delete Sprint | `sprints.delete` | `sprints.delete` | ⚠️ Needs Audit | Check Sprints.tsx |
| Start Sprint | `sprints.manage` | `sprints.manage` | ✅ Fixed | Sprints.tsx - Start button guarded |
| Complete Sprint | `sprints.manage` | `sprints.manage` | ✅ Fixed | Sprints.tsx - Complete button guarded |
| Add Tasks to Sprint | `sprints.edit` | `sprints.edit` | ✅ Fixed | Sprints.tsx - Add Tasks button guarded |
| View Sprints | `sprints.view` | `sprints.view` | ✅ OK | Default permission |

### Defects

| Action | Frontend Permission | Backend Permission | Status | Notes |
|--------|-------------------|-------------------|--------|-------|
| Create Defect | `defects.create` | `defects.create` | ✅ Fixed | Defects.tsx - Report Defect button guarded |
| Edit Defect | `defects.edit` | `defects.edit` | ⚠️ Needs Audit | Check DefectDetailSheet.tsx |
| Delete Defect | `defects.delete` | `defects.delete` | ⚠️ Needs Audit | Check DefectDetailSheet.tsx |
| View Defects | `defects.view` | `defects.view` | ✅ OK | Default permission |

### Backlog

| Action | Frontend Permission | Backend Permission | Status | Notes |
|--------|-------------------|-------------------|--------|-------|
| Create Backlog Item | `backlog.create` | `backlog.create` | ⚠️ Needs Audit | Check Backlog.tsx |
| Edit Backlog Item | `backlog.edit` | `backlog.edit` | ⚠️ Needs Audit | Check Backlog.tsx |
| Delete Backlog Item | `backlog.delete` | `backlog.delete` | ⚠️ Needs Audit | Check Backlog.tsx |
| View Backlog | `backlog.view` | `backlog.view` | ✅ OK | Default permission |

### Users & Admin

| Action | Frontend Permission | Backend Permission | Status | Notes |
|--------|-------------------|-------------------|--------|-------|
| Manage Users | `users.manage` | `users.*` | ⚠️ Needs Audit | Check AdminUsers.tsx |
| Create User | `users.create` | `users.create` | ⚠️ Needs Audit | Check InviteUserDialog.tsx |
| Edit User | `users.update` | `users.update` | ⚠️ Needs Audit | Check EditUserDialog.tsx |
| Delete User | `users.delete` | `users.delete` | ⚠️ Needs Audit | Check DeleteUserDialog.tsx |
| Admin Settings | `admin.settings.update` | `admin.settings.update` | ✅ OK | Uses RequireAdminGuard |
| Admin Permissions | `admin.permissions.update` | `admin.permissions.update` | ✅ OK | Uses RequireAdminGuard |

### Comments & Attachments

| Action | Frontend Permission | Backend Permission | Status | Notes |
|--------|-------------------|-------------------|--------|-------|
| Add Comment | `tasks.comment` | `tasks.comment` | ⚠️ Needs Audit | Check CommentForm.tsx |
| Edit Comment | `tasks.comment` | `tasks.comment` | ⚠️ Needs Audit | Check CommentItem.tsx |
| Delete Comment | `tasks.comment` | `tasks.comment` | ⚠️ Needs Audit | Check CommentItem.tsx |
| Upload Attachment | Context-dependent | Context-dependent | ⚠️ Needs Audit | Check AttachmentUpload.tsx |
| Delete Attachment | Context-dependent | Context-dependent | ⚠️ Needs Audit | Check AttachmentList.tsx |

### Milestones & Releases

| Action | Frontend Permission | Backend Permission | Status | Notes |
|--------|-------------------|-------------------|--------|-------|
| Create Milestone | Project-level | Project-level | ⚠️ Needs Audit | Check CreateMilestoneDialog.tsx |
| Edit Milestone | Project-level | Project-level | ⚠️ Needs Audit | Check EditMilestoneDialog.tsx |
| Delete Milestone | Project-level | Project-level | ⚠️ Needs Audit | Check MilestoneCard.tsx |
| Create Release | Project-level | Project-level | ⚠️ Needs Audit | Check CreateReleaseDialog.tsx |
| Edit Release | Project-level | Project-level | ⚠️ Needs Audit | Check EditReleaseDialog.tsx |
| Delete Release | Project-level | Project-level | ⚠️ Needs Audit | Check ReleaseCard.tsx |

---

## Component Audit

### ✅ Fixed Components

1. **Projects.tsx**
   - ✅ Create Project button - Uses `PermissionGuard` with `projects.create`
   - ✅ Edit Project menu item - Uses `PermissionGuard` with `projects.edit` + projectId
   - ✅ Delete Project menu item - Uses `PermissionGuard` with `projects.delete` + projectId
   - ✅ Archive Project menu item - Uses `PermissionGuard` with `projects.delete` + projectId

2. **Sprints.tsx**
   - ✅ Create Sprint button - Uses `PermissionGuard` with `sprints.create` + projectId
   - ✅ Start Sprint button - Uses `PermissionGuard` with `sprints.manage` + projectId
   - ✅ Complete Sprint button - Uses `PermissionGuard` with `sprints.manage` + projectId
   - ✅ Add Tasks button - Uses `PermissionGuard` with `sprints.edit` + projectId

3. **Defects.tsx**
   - ✅ Report Defect button - Uses `PermissionGuard` with `defects.create` + projectId

### ⚠️ Components Needing Audit

#### Pages

1. **Tasks.tsx**
   - ⚠️ Create Task button - Check if guarded
   - ⚠️ Task actions (edit, delete, assign) - Check TaskDetailSheet.tsx

2. **Backlog.tsx**
   - ⚠️ All backlog item actions - Needs full audit

3. **ProjectDetail.tsx**
   - ⚠️ Edit Project button
   - ⚠️ Delete Project button
   - ⚠️ Create Milestone button
   - ⚠️ Create Release button

4. **ProjectMembers.tsx**
   - ⚠️ Invite Member button
   - ⚠️ Remove Member actions
   - ⚠️ Change Role actions

5. **Admin Pages**
   - ⚠️ AdminUsers.tsx - All user management actions
   - ⚠️ AdminSettings.tsx - Settings update actions
   - ⚠️ AdminOrganizations.tsx - Organization management

#### Dialogs

1. **CreateTaskDialog.tsx** - Check if permission checked before opening
2. **EditProjectDialog.tsx** - Check if permission checked before opening
3. **DeleteProjectDialog.tsx** - Check if permission checked before opening
4. **CreateDefectDialog.tsx** - Check if permission checked before opening
5. **EditDefectDialog.tsx** - Check if exists and permission checked
6. **CreateMilestoneDialog.tsx** - Check permission
7. **EditMilestoneDialog.tsx** - Check permission
8. **CreateReleaseDialog.tsx** - Check permission
9. **EditReleaseDialog.tsx** - Check permission
10. **InviteUserDialog.tsx** - Check `users.create` permission
11. **EditUserDialog.tsx** - Check `users.update` permission
12. **DeleteUserDialog.tsx** - Check `users.delete` permission

#### Components

1. **TaskDetailSheet.tsx**
   - ⚠️ Edit button
   - ⚠️ Delete button
   - ⚠️ Assign dropdown

2. **TaskListView.tsx**
   - ⚠️ Delete task action

3. **DefectDetailSheet.tsx**
   - ⚠️ Delete button

4. **CommentForm.tsx**
   - ⚠️ Submit button - Check `tasks.comment` permission

5. **CommentItem.tsx**
   - ⚠️ Edit button - Check `tasks.comment` permission
   - ⚠️ Delete button - Check `tasks.comment` permission

6. **AttachmentUpload.tsx**
   - ⚠️ Upload button - Check context-specific permission

7. **AttachmentList.tsx**
   - ⚠️ Delete button - Check context-specific permission

8. **MilestoneCard.tsx**
   - ⚠️ Edit button
   - ⚠️ Delete button

9. **ReleaseCard.tsx**
   - ⚠️ Edit button
   - ⚠️ Delete button

---

## Remaining Work

### High Priority

1. **Task Management**
   - [ ] Audit Tasks.tsx - Create Task button
   - [ ] Audit TaskDetailSheet.tsx - Edit, Delete, Assign actions
   - [ ] Audit TaskListView.tsx - Delete action
   - [ ] Audit CreateTaskDialog.tsx - Permission check before opening

2. **Project Management**
   - [ ] Audit ProjectDetail.tsx - All action buttons
   - [ ] Audit ProjectMembers.tsx - Member management actions
   - [ ] Audit EditProjectDialog.tsx - Permission check
   - [ ] Audit DeleteProjectDialog.tsx - Permission check

3. **Comments & Attachments**
   - [ ] Audit CommentForm.tsx - Submit button
   - [ ] Audit CommentItem.tsx - Edit/Delete buttons
   - [ ] Audit AttachmentUpload.tsx - Upload button
   - [ ] Audit AttachmentList.tsx - Delete button

### Medium Priority

4. **Backlog**
   - [ ] Audit Backlog.tsx - All backlog item actions

5. **Milestones & Releases**
   - [ ] Audit all milestone dialogs and components
   - [ ] Audit all release dialogs and components

6. **Admin Pages**
   - [ ] Audit AdminUsers.tsx
   - [ ] Audit AdminOrganizations.tsx
   - [ ] Audit AdminSettings.tsx

### Low Priority

7. **Sprint Management**
   - [ ] Audit Edit Sprint functionality (if exists)
   - [ ] Audit Delete Sprint functionality (if exists)

---

## Best Practices

### When to Use PermissionGuard

✅ **Use PermissionGuard when:**
- Hiding entire UI sections (buttons, forms, menus)
- You want to completely hide the element (not just disable)
- The element is standalone and doesn't need tooltips

```tsx
<PermissionGuard 
  requiredPermission="projects.create" 
  fallback={null}
  showNotification={false}
>
  <Button>Create Project</Button>
</PermissionGuard>
```

### When to Use PermissionButton

✅ **Use PermissionButton when:**
- You want to show the button but disable it with a tooltip
- The button should remain visible for UX reasons
- You want to explain why the action is unavailable

```tsx
<PermissionButton
  hasPermission={can('projects.create')}
  permissionName="projects.create"
  disabledReason="You need Create Projects permission"
>
  Create Project
</PermissionButton>
```

### When to Use usePermissions Hook Directly

✅ **Use usePermissions directly when:**
- Conditional rendering logic is complex
- You need to check multiple permissions
- You need permission checks in event handlers

```tsx
const { can } = usePermissions();
const canCreate = can('projects.create');
const canEdit = can('projects.edit');

if (canCreate || canEdit) {
  return <ActionPanel />;
}
```

### Project-Specific Permissions

✅ **Always use projectId for project-scoped actions:**

```tsx
<PermissionGuard 
  requiredPermission="projects.edit" 
  projectId={projectId}  // ← Important!
  fallback={null}
>
  <EditButton />
</PermissionGuard>
```

### Error Handling

✅ **403 errors are automatically handled:**
- API client (`api/client.ts`) catches 403 responses
- Extracts permission error messages from backend
- Shows user-friendly toast notifications
- Formats permission names (e.g., "projects.create" → "Create Projects")

---

## Testing Checklist

- [ ] Test with Admin user - should see all buttons
- [ ] Test with regular User - should see limited buttons
- [ ] Test with Viewer role - should see read-only UI
- [ ] Test with ProductOwner role - should see all project actions
- [ ] Test with ScrumMaster role - should see sprint management
- [ ] Test with Developer role - should see task actions but not project management
- [ ] Test permission denied scenarios - buttons should be hidden, not just disabled
- [ ] Test 403 error handling - should show user-friendly messages
- [ ] Test project-specific permissions - different projects should show different buttons

---

## Notes

- **Permission naming:** Frontend and backend use the same format: `resource.action` (e.g., `projects.create`)
- **Admin bypass:** Admins automatically have all permissions (checked in `usePermissions` hook)
- **Project roles:** Project-specific permissions are calculated from project role (ProductOwner, ScrumMaster, Developer, Tester, Viewer)
- **Fallback behavior:** When `fallback={null}` and no `redirectTo`, PermissionGuard returns `null` (hides element)
- **Notification behavior:** Set `showNotification={false}` when using PermissionGuard in dropdowns/menus to avoid duplicate notifications

---

## Changelog

### 2025-01-XX - Initial Audit
- ✅ Created useUserRole hook
- ✅ Enhanced PermissionGuard with tooltip support
- ✅ Added permission helper functions to utils.ts
- ✅ Enhanced 403 error handling in API client
- ✅ Fixed Projects.tsx - Create, Edit, Delete buttons
- ✅ Fixed Sprints.tsx - Create, Start, Complete, Add Tasks buttons
- ✅ Fixed Defects.tsx - Report Defect button
- ⚠️ Identified remaining components needing audit

---

**Last Updated:** January 2025  
**Next Review:** After completing remaining component audits

