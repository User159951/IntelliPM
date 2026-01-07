# IntelliPM Permission Matrix

This document provides a comprehensive overview of all permissions across project-level roles in IntelliPM.

## Project-Level Roles

1. **ProductOwner** - Full project control, strategic decisions
2. **ScrumMaster** - Sprint management, team coordination
3. **Developer** - Technical implementation, task management
4. **Tester** - Quality assurance, release validation
5. **Viewer** - Read-only access
6. **Manager** - Read all, comment, validate milestones (no technical modifications)

---

## Permission Matrix

### Projects

| Permission | ProductOwner | ScrumMaster | Developer | Tester | Viewer | Manager |
|------------|--------------|-------------|-----------|--------|--------|---------|
| `projects.view` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `projects.create` | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `projects.update` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `projects.edit` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `projects.delete` | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `projects.members.invite` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `projects.members.remove` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `projects.members.changeRole` | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |

### Tasks

| Permission | ProductOwner | ScrumMaster | Developer | Tester | Viewer | Manager |
|------------|--------------|-------------|-----------|--------|--------|---------|
| `tasks.view` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `tasks.create` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `tasks.update` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `tasks.edit` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `tasks.assign` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `tasks.comment` | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| `tasks.delete` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `tasks.dependencies.create` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `tasks.dependencies.delete` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |

### Sprints

| Permission | ProductOwner | ScrumMaster | Developer | Tester | Viewer | Manager |
|------------|--------------|-------------|-----------|--------|--------|---------|
| `sprints.view` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `sprints.create` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `sprints.update` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `sprints.delete` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `sprints.manage` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `sprints.start` | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `sprints.close` | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |

**Note:** `sprints.start` and `sprints.close` are **exclusively** for ScrumMaster role. ProductOwner cannot start/close sprints directly.

### Releases

| Permission | ProductOwner | ScrumMaster | Developer | Tester | Viewer | Manager |
|------------|--------------|-------------|-----------|--------|--------|---------|
| `releases.view` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `releases.create` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `releases.edit` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `releases.delete` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `releases.deploy` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `releases.approve` | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| `releases.notes.edit` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |

**Note:** `releases.approve` is **exclusively** for Tester/QA role. QA approval is required before deployment.

### Quality Gates

| Permission | ProductOwner | ScrumMaster | Developer | Tester | Viewer | Manager |
|------------|--------------|-------------|-----------|--------|--------|---------|
| `quality-gates.view` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `quality-gates.validate` | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |

**Note:** `quality-gates.validate` is **exclusively** for Tester/QA role. QA must validate quality gates before release approval.

### Milestones

| Permission | ProductOwner | ScrumMaster | Developer | Tester | Viewer | Manager |
|------------|--------------|-------------|-----------|--------|--------|---------|
| `milestones.view` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `milestones.create` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `milestones.edit` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `milestones.complete` | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ |
| `milestones.delete` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `milestones.validate` | ✅ | ✅ | ❌ | ❌ | ❌ | ✅ |

**Note:** `milestones.validate` allows Manager and ProductOwner/ScrumMaster to validate milestone completion.

### Backlog

| Permission | ProductOwner | ScrumMaster | Developer | Tester | Viewer | Manager |
|------------|--------------|-------------|-----------|--------|--------|---------|
| `backlog.view` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `backlog.create` | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| `backlog.edit` | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| `backlog.delete` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |

### Defects

| Permission | ProductOwner | ScrumMaster | Developer | Tester | Viewer | Manager |
|------------|--------------|-------------|-----------|--------|--------|---------|
| `defects.view` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `defects.create` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `defects.edit` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `defects.delete` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |

### Teams

| Permission | ProductOwner | ScrumMaster | Developer | Tester | Viewer | Manager |
|------------|--------------|-------------|-----------|--------|--------|---------|
| `teams.view` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `teams.create` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `teams.edit` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `teams.view.availability` | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |

### Activity & Search

| Permission | ProductOwner | ScrumMaster | Developer | Tester | Viewer | Manager |
|------------|--------------|-------------|-----------|--------|--------|---------|
| `activity.view` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `search.use` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

### Metrics & Insights

| Permission | ProductOwner | ScrumMaster | Developer | Tester | Viewer | Manager |
|------------|--------------|-------------|-----------|--------|--------|---------|
| `metrics.view` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| `insights.view` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

### AI

| Permission | ProductOwner | ScrumMaster | Developer | Tester | Viewer | Manager |
|------------|--------------|-------------|-----------|--------|--------|---------|
| `ai.use` | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

---

## Role Summaries

### ProductOwner
- **Full project control**: Create, edit, delete projects
- **Member management**: Invite, remove, change roles
- **Sprint management**: Create, edit sprints (but cannot start/close - ScrumMaster exclusive)
- **Release management**: Create, edit, deploy releases
- **Backlog management**: Full control
- **Task management**: Full control including deletion
- **Milestone management**: Full control

### ScrumMaster
- **Sprint control**: **EXCLUSIVE** permission to start and close sprints
- **Sprint management**: Create, edit, delete sprints
- **Release management**: Create, edit, deploy releases (but cannot approve - QA exclusive)
- **Task management**: Full control including deletion
- **Backlog management**: Create, edit, delete
- **Member management**: Invite and remove (but cannot change roles)
- **Project editing**: Can edit project settings

### Developer
- **Task management**: Create, edit, assign, comment, manage dependencies
- **Backlog**: Create and edit items
- **Defects**: Create and edit
- **View access**: Sprints, releases, milestones, teams, metrics, insights
- **No permissions**: Cannot delete tasks, cannot manage sprints/releases, cannot change member roles

### Tester (QA)
- **Quality assurance**: **EXCLUSIVE** permission to approve releases and validate quality gates
- **Defect management**: Create and edit defects
- **Task management**: Create, edit, assign, comment
- **View access**: Full read access to all project data
- **No permissions**: Cannot manage sprints/releases (except approval), cannot delete tasks

### Viewer
- **Read-only**: View all project data
- **No modifications**: Cannot create, edit, or delete anything
- **No comments**: Cannot comment on tasks

### Manager
- **Read access**: View all project data
- **Comments**: Can comment on tasks
- **Milestone validation**: Can validate and complete milestones
- **No technical modifications**: Cannot create/edit tasks, sprints, releases, or backlog items
- **No deletions**: Cannot delete anything

---

## Permission Enforcement Rules

### Strict Enforcement
1. **Sprint Start/Close**: Only ScrumMaster can start or close sprints. ProductOwner cannot bypass this.
2. **Release Approval**: Only Tester/QA can approve releases. This is a required gatekeeper step.
3. **Quality Gate Validation**: Only Tester/QA can validate quality gates before release approval.
4. **Role-Based Checks**: All command handlers validate user roles before executing operations.
5. **No Bypass**: Permission checks are enforced at the handler level, preventing any bypass attempts.

### Validation Points
- **Command Handlers**: All handlers check permissions before executing
- **API Controllers**: Controllers use `[RequirePermission]` attributes
- **Domain Logic**: Domain methods validate role-based operations
- **Database Level**: RolePermission table enforces global-level permissions

---

## Global-Level Roles

Global roles (User, Admin, SuperAdmin) control organization-wide permissions and are separate from project-level roles. See `GlobalPermissions.cs` for details.

---

## Notes

- **Permission Inheritance**: ProductOwner has all ScrumMaster permissions plus additional ones
- **Exclusive Permissions**: Some permissions are role-exclusive and cannot be delegated
- **QA Gatekeeper**: QA/Tester role is required for release approval workflow
- **ScrumMaster Authority**: Sprint lifecycle is controlled exclusively by ScrumMaster
- **Manager Role**: Designed for stakeholders who need visibility and milestone validation without technical access

---

*Last Updated: 2025-01-06*
*Version: 2.0*

