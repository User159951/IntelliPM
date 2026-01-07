# IntelliPM Roles and Permissions Guide

## Overview

IntelliPM uses a two-tier role system:
- **Global Roles**: Organization-wide permissions (User, Admin, SuperAdmin)
- **Project Roles**: Project-specific permissions (ProductOwner, ScrumMaster, Developer, Tester, Viewer, Manager)

Users have one global role and can have different project roles in different projects.

---

## Global Roles

### User
- **Purpose**: Standard user with basic permissions
- **Capabilities**:
  - View projects they're members of
  - Create new projects (unless restricted)
  - Access basic features based on project role
- **Limitations**: Cannot manage organization settings or other users

### Admin
- **Purpose**: Organization administrator managing their own organization
- **Capabilities**:
  - All User permissions
  - Manage organization settings
  - Manage users within their organization
  - View admin panel
  - Configure AI quotas and governance
- **Limitations**: Cannot access other organizations' data

### SuperAdmin
- **Purpose**: System administrator with full system access
- **Capabilities**:
  - All Admin permissions
  - Access all organizations
  - Manage system-wide settings
  - Approve AI decisions from any organization
  - System health monitoring
- **Limitations**: None (full system access)

---

## Project Roles

### ProductOwner
- **Purpose**: Owns product vision, prioritizes backlog, makes strategic product decisions
- **Key Responsibilities**:
  - Define product roadmap and features
  - Prioritize backlog items
  - Make product decisions
  - Manage project members and roles
- **What They Can Do**:
  - ✅ Create, edit, and delete projects
  - ✅ Invite/remove members and change their roles
  - ✅ Create, edit, and delete sprints (but cannot start/close - ScrumMaster exclusive)
  - ✅ Create, edit, and deploy releases
  - ✅ Full backlog management
  - ✅ Full task management including deletion
  - ✅ Create and manage milestones
- **What They Cannot Do**:
  - ❌ Start or close sprints (ScrumMaster exclusive)
  - ❌ Approve releases (Tester/QA exclusive)
- **Example**: "As a ProductOwner, I can create a new sprint and add tasks to it, but I need a ScrumMaster to start the sprint."

### ScrumMaster
- **Purpose**: Facilitates Scrum process, manages sprints, removes impediments
- **Key Responsibilities**:
  - Start and close sprints
  - Facilitate sprint ceremonies
  - Remove blockers
  - Coordinate team activities
- **What They Can Do**:
  - ✅ **EXCLUSIVE**: Start and close sprints
  - ✅ Create, edit, and delete sprints
  - ✅ Create, edit, and deploy releases (but cannot approve - QA exclusive)
  - ✅ Full task management including deletion
  - ✅ Invite/remove project members
  - ✅ Edit project settings
  - ✅ Manage backlog items
- **What They Cannot Do**:
  - ❌ Change member roles (ProductOwner exclusive)
  - ❌ Delete projects (ProductOwner exclusive)
  - ❌ Approve releases (Tester/QA exclusive)
- **Example**: "As a ScrumMaster, I can start sprints and manage sprint tasks, but I cannot approve releases for deployment."

### Developer
- **Purpose**: Technical implementation, writes code, completes tasks
- **Key Responsibilities**:
  - Implement features
  - Complete assigned tasks
  - Update task status
  - Create and manage task dependencies
- **What They Can Do**:
  - ✅ Create, edit, and assign tasks
  - ✅ Change task status (Todo → InProgress → InReview)
  - ✅ Comment on tasks
  - ✅ Create and edit backlog items
  - ✅ Create and edit defects
  - ✅ View all project data (sprints, releases, milestones, teams, metrics)
- **What They Cannot Do**:
  - ❌ Delete tasks
  - ❌ Start or close sprints
  - ❌ Create or manage sprints/releases
  - ❌ Change member roles
  - ❌ Approve releases
- **Example**: "As a Developer, I can create tasks and move them to InProgress, but I cannot start sprints or approve releases."

### Tester (QA)
- **Purpose**: Quality assurance, release validation, testing
- **Key Responsibilities**:
  - Test features and releases
  - Validate quality gates
  - Approve releases for deployment
  - Create and manage defects
- **What They Can Do**:
  - ✅ **EXCLUSIVE**: Approve releases for deployment
  - ✅ **EXCLUSIVE**: Validate quality gates
  - ✅ Create, edit, and assign tasks
  - ✅ Change task status (can mark tasks as Done after review)
  - ✅ Create and edit defects
  - ✅ View all project data
- **What They Cannot Do**:
  - ❌ Delete tasks
  - ❌ Start or close sprints
  - ❌ Create or manage sprints/releases (except approval)
  - ❌ Deploy releases (can only approve)
- **Example**: "As a Tester, I can approve releases and validate quality gates, but I cannot start sprints or deploy releases."

### Viewer
- **Purpose**: Read-only access for stakeholders and observers
- **Key Responsibilities**:
  - View project progress
  - Monitor metrics and insights
  - Stay informed about project status
- **What They Can Do**:
  - ✅ View all project data (projects, tasks, sprints, releases, backlog, defects, milestones, teams, metrics, insights)
  - ✅ Use search functionality
  - ✅ View activity feed
- **What They Cannot Do**:
  - ❌ Create, edit, or delete anything
  - ❌ Comment on tasks
  - ❌ Assign tasks
  - ❌ Change any status
- **Example**: "As a Viewer, I can see all project information but cannot make any changes or comments."

### Manager
- **Purpose**: Stakeholder visibility and milestone validation without technical access
- **Key Responsibilities**:
  - Monitor project progress
  - Validate milestone completion
  - Provide feedback through comments
- **What They Can Do**:
  - ✅ View all project data
  - ✅ Comment on tasks
  - ✅ Validate and complete milestones
  - ✅ View team availability
- **What They Cannot Do**:
  - ❌ Create or edit tasks, sprints, releases, or backlog items
  - ❌ Delete anything
  - ❌ Change task status
  - ❌ Assign tasks
- **Example**: "As a Manager, I can validate milestones and comment on tasks, but I cannot create or modify technical items."

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

## Role Examples

### Example 1: Developer Workflow
**Scenario**: A Developer wants to work on a task.

**What they can do**:
- ✅ View the task in the backlog
- ✅ Assign the task to themselves
- ✅ Change status from Todo → InProgress
- ✅ Update task details and add comments
- ✅ Change status from InProgress → InReview when done

**What they cannot do**:
- ❌ Start the sprint (ScrumMaster only)
- ❌ Mark task as Done (Tester/ScrumMaster only)
- ❌ Delete the task
- ❌ Create a new sprint

### Example 2: ScrumMaster Workflow
**Scenario**: A ScrumMaster wants to start a new sprint.

**What they can do**:
- ✅ Create a new sprint
- ✅ Add tasks to the sprint
- ✅ Set sprint dates and goals
- ✅ **EXCLUSIVE**: Start the sprint
- ✅ **EXCLUSIVE**: Close the sprint when complete

**What they cannot do**:
- ❌ Change member roles (ProductOwner only)
- ❌ Approve releases (Tester/QA only)
- ❌ Delete the project (ProductOwner only)

### Example 3: Tester Workflow
**Scenario**: A Tester needs to approve a release.

**What they can do**:
- ✅ View the release and its quality gates
- ✅ **EXCLUSIVE**: Validate quality gates
- ✅ **EXCLUSIVE**: Approve the release for deployment
- ✅ Create defects if issues are found
- ✅ Test features and provide feedback

**What they cannot do**:
- ❌ Deploy the release (ProductOwner/ScrumMaster only)
- ❌ Start or close sprints
- ❌ Create or edit sprints/releases
- ❌ Delete tasks

### Example 4: ProductOwner Workflow
**Scenario**: A ProductOwner wants to manage a project.

**What they can do**:
- ✅ Create and configure the project
- ✅ Invite team members and assign roles
- ✅ Prioritize backlog items
- ✅ Create sprints and releases
- ✅ Deploy releases (after QA approval)
- ✅ Delete projects and tasks

**What they cannot do**:
- ❌ Start or close sprints (ScrumMaster exclusive)
- ❌ Approve releases (Tester/QA exclusive)

---

## Permission Enforcement

### Enforcement Points

1. **Backend API**: All endpoints check permissions using `[RequirePermission]` attributes
2. **Command Handlers**: Domain logic validates roles before executing operations
3. **Frontend Guards**: `PermissionGuard` component hides UI elements based on permissions
4. **Workflow Validators**: Status transitions check role permissions

### Exclusive Permissions

Some permissions are **role-exclusive** and cannot be delegated:

1. **Sprint Start/Close**: Only ScrumMaster
   - Enforced at backend handler level
   - ProductOwner cannot bypass this restriction

2. **Release Approval**: Only Tester/QA
   - Required gatekeeper step before deployment
   - Cannot be bypassed by ProductOwner or ScrumMaster

3. **Quality Gate Validation**: Only Tester/QA
   - Required before release approval
   - Ensures quality standards are met

4. **Role Changes**: Only ProductOwner
   - Prevents unauthorized privilege escalation
   - Maintains organizational hierarchy

### Permission Inheritance

- **ProductOwner** inherits all ScrumMaster permissions plus additional ones
- **Admin** has all permissions automatically
- **SuperAdmin** has all permissions across all organizations

---

## Common Permission Scenarios

### Scenario 1: "I can't start a sprint"
**Problem**: User is ProductOwner but cannot start sprints.

**Solution**: This is by design. Only ScrumMaster can start sprints. ProductOwner should:
- Create the sprint
- Add tasks to the sprint
- Ask a ScrumMaster to start it
- Or assign themselves ScrumMaster role in addition to ProductOwner

### Scenario 2: "I can't approve a release"
**Problem**: User is ProductOwner but cannot approve releases.

**Solution**: This is by design. Only Tester/QA can approve releases. ProductOwner should:
- Create and configure the release
- Ask a Tester/QA to validate quality gates and approve
- Then deploy the release

### Scenario 3: "I can't delete a task"
**Problem**: User is Developer but cannot delete tasks.

**Solution**: This is by design. Only ProductOwner and ScrumMaster can delete tasks. Developers should:
- Mark tasks as Done instead
- Or ask ProductOwner/ScrumMaster to delete if necessary

### Scenario 4: "I can't comment on tasks"
**Problem**: User is Viewer but cannot comment.

**Solution**: This is by design. Viewers have read-only access. To comment, user needs:
- Developer, Tester, ScrumMaster, or ProductOwner role
- Or Manager role (can comment but not edit)

---

## Best Practices

1. **Principle of Least Privilege**: Assign the minimum role needed for a user's responsibilities
2. **Role Separation**: Use different roles for different responsibilities (e.g., separate ScrumMaster and ProductOwner)
3. **Regular Audits**: Review user roles periodically to ensure they match current responsibilities
4. **Clear Communication**: Explain role restrictions to team members to avoid confusion
5. **Workflow Awareness**: Understand which roles are needed for each workflow step

---

## Troubleshooting

### "Access Denied" Errors

1. **Check your role**: Verify your project role in Project Settings → Members
2. **Check permissions**: Some actions require specific permissions (see matrix above)
3. **Check global role**: Admin users have all permissions automatically
4. **Contact admin**: If you believe you should have access, contact your organization admin

### Permission Not Working

1. **Clear cache**: Permissions are cached - try refreshing the page
2. **Check project context**: Some permissions are project-specific
3. **Verify backend**: Backend enforces permissions - frontend checks are for UX only
4. **Check workflow rules**: Some status transitions have additional role requirements

---

*Last Updated: 2025-01-06*
*Version: 1.0*

