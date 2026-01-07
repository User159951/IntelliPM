# IntelliPM Workflow Guide

## Overview

IntelliPM implements structured workflows for tasks, sprints, and releases. Each workflow has specific status transitions with role-based requirements. Understanding these workflows is essential for effective project management.

---

## Task Workflow

### Status Flow

```
Todo → InProgress → InReview → Done
  ↓         ↓
Blocked ← Blocked
```

### Status Definitions

- **Todo**: Task is created but not yet started
- **InProgress**: Task is actively being worked on
- **InReview**: Task is completed and ready for review/testing
- **Done**: Task is completed and approved
- **Blocked**: Task cannot proceed due to dependencies or issues

### Status Transitions

#### Todo → InProgress
- **Allowed Roles**: Developer, Tester, ScrumMaster, ProductOwner
- **Description**: Any team member can start working on a task
- **Example**: "As a Developer, I can move a task from Todo to InProgress when I start working on it."

#### Todo → Blocked
- **Allowed Roles**: Developer, Tester, ScrumMaster, ProductOwner
- **Description**: Any team member can mark a task as blocked
- **Example**: "If a task has a dependency that's not ready, I can mark it as Blocked."

#### InProgress → InReview
- **Allowed Roles**: Developer, ScrumMaster
- **Description**: Developers mark tasks as ready for review when implementation is complete
- **Example**: "As a Developer, I move a task to InReview when my code is ready for testing."

#### InProgress → Done
- **Allowed Roles**: Tester, ScrumMaster, ProductOwner
- **Required Conditions**: QA Approval
- **Description**: Allows skipping review phase (e.g., for simple tasks)
- **Example**: "A Tester can mark a simple task as Done directly if no review is needed."

#### InProgress → Blocked
- **Allowed Roles**: Developer, Tester, ScrumMaster, ProductOwner
- **Description**: Any team member can block an in-progress task
- **Example**: "If I discover a blocker while working, I can mark the task as Blocked."

#### InReview → Done
- **Allowed Roles**: Tester, ScrumMaster, ProductOwner
- **Required Conditions**: QA Approval
- **Description**: Testers approve reviewed tasks as complete
- **Example**: "As a Tester, I mark tasks as Done after validating they meet quality standards."

#### InReview → InProgress
- **Allowed Roles**: Tester, ScrumMaster
- **Description**: Testers can send tasks back to development if issues are found
- **Example**: "If testing reveals issues, I can move the task back to InProgress for fixes."

#### Blocked → Todo
- **Allowed Roles**: Developer, Tester, ScrumMaster, ProductOwner
- **Description**: Any team member can unblock a task
- **Example**: "Once a blocker is resolved, I can move the task back to Todo."

#### Blocked → InProgress
- **Allowed Roles**: Developer, Tester, ScrumMaster, ProductOwner
- **Description**: Resume work on a previously blocked task
- **Example**: "If a blocker is resolved and I'm ready to continue, I can move directly to InProgress."

#### Done → InReview
- **Allowed Roles**: ScrumMaster, ProductOwner
- **Description**: Reopen completed tasks for corrections (rare)
- **Example**: "If a bug is found in a completed task, ScrumMaster can reopen it for fixes."

### Task Workflow Examples

#### Example 1: Standard Development Flow
1. **ProductOwner** creates task → Status: **Todo**
2. **Developer** assigns task to themselves → Status: **Todo**
3. **Developer** starts work → Status: **InProgress**
4. **Developer** completes implementation → Status: **InReview**
5. **Tester** validates and approves → Status: **Done**

#### Example 2: Blocked Task Flow
1. **Developer** starts task → Status: **InProgress**
2. **Developer** discovers blocker → Status: **Blocked**
3. Blocker resolved → Status: **Todo** or **InProgress**
4. **Developer** continues work → Status: **InProgress**
5. **Developer** completes → Status: **InReview**
6. **Tester** approves → Status: **Done**

#### Example 3: Fast-Track Flow
1. **Developer** starts simple task → Status: **InProgress**
2. **Tester** validates immediately → Status: **Done** (skips InReview)

---

## Sprint Workflow

### Status Flow

```
NotStarted → Active → Completed
     ↓          ↓
  Cancelled  Cancelled
```

### Status Definitions

- **NotStarted**: Sprint is created but not yet active
- **Active**: Sprint is currently running
- **Completed**: Sprint has finished
- **Cancelled**: Sprint was cancelled before completion

### Status Transitions

#### NotStarted → Active
- **Allowed Roles**: ScrumMaster **ONLY**
- **Required Conditions**:
  - Sprint must have at least 1 task
  - Sprint must have dates defined
  - No other active sprint in the project
- **Description**: Only ScrumMaster can start sprints
- **Example**: "As a ScrumMaster, I start the sprint when planning is complete and tasks are ready."

#### Active → Completed
- **Allowed Roles**: ScrumMaster **ONLY**
- **Required Conditions**: AllTasksCompleted (all tasks in sprint are Done)
- **Description**: ScrumMaster closes the sprint when all work is done
- **Example**: "As a ScrumMaster, I close the sprint after the sprint review and retrospective."

#### Active → Cancelled
- **Allowed Roles**: ScrumMaster, ProductOwner
- **Description**: Cancel an active sprint if needed (e.g., major priority change)
- **Example**: "If priorities change dramatically, ProductOwner can cancel the sprint."

#### Completed → Active
- **Allowed Roles**: ScrumMaster, ProductOwner
- **Description**: Reopen a completed sprint for corrections (rare)
- **Example**: "If critical issues are found, ScrumMaster can reopen the sprint."

### Sprint Workflow Examples

#### Example 1: Standard Sprint Flow
1. **ProductOwner** creates sprint → Status: **NotStarted**
2. **ProductOwner** adds tasks to sprint → Status: **NotStarted**
3. **ScrumMaster** starts sprint → Status: **Active**
4. Team works on tasks during sprint → Status: **Active**
5. **ScrumMaster** closes sprint → Status: **Completed**

#### Example 2: Cancelled Sprint Flow
1. **ProductOwner** creates sprint → Status: **NotStarted**
2. **ScrumMaster** starts sprint → Status: **Active**
3. Major priority change occurs → Status: **Cancelled**
4. New sprint created with updated priorities

### Sprint Lifecycle Rules

1. **Only one active sprint per project**: Starting a new sprint requires closing the current one
2. **Tasks must be in sprint**: Sprint must have at least one task before starting
3. **Dates required**: Sprint must have start and end dates before starting
4. **Completion requires all tasks done**: Sprint cannot be completed if tasks are still in progress

---

## Release Workflow

### Status Flow

```
Planned → InProgress → Testing → ReadyForDeployment → Deployed
    ↓         ↓           ↓
Cancelled  Cancelled  Cancelled
```

### Status Definitions

- **Planned**: Release is planned but not started
- **InProgress**: Release is being prepared
- **Testing**: Release is in testing phase
- **ReadyForDeployment**: Release passed testing and is ready to deploy
- **Deployed**: Release has been deployed
- **Failed**: Release deployment failed
- **Cancelled**: Release was cancelled

### Status Transitions

#### Planned → InProgress
- **Allowed Roles**: ProductOwner, ScrumMaster
- **Description**: Start preparing the release
- **Example**: "ProductOwner moves release to InProgress when development is ready."

#### InProgress → Testing
- **Allowed Roles**: ProductOwner, ScrumMaster
- **Description**: Mark release as ready for testing
- **Example**: "ScrumMaster moves release to Testing when all features are complete."

#### Testing → ReadyForDeployment
- **Allowed Roles**: Tester, ScrumMaster
- **Required Conditions**: QualityGatesPassed
- **Description**: Tester approves release after quality gates pass
- **Example**: "As a Tester, I approve the release after validating all quality gates."

#### ReadyForDeployment → Deployed
- **Allowed Roles**: ProductOwner, ScrumMaster
- **Required Conditions**:
  - QA approval (at least one quality gate validated by Tester)
  - All blocking quality gates passed
- **Description**: Deploy the approved release
- **Example**: "ProductOwner deploys the release after QA approval."

#### Any Status → Cancelled
- **Allowed Roles**: ProductOwner, ScrumMaster
- **Description**: Cancel release if needed
- **Example**: "If critical issues are found, ProductOwner can cancel the release."

### Quality Gates

Quality gates are checkpoints that must pass before release approval:

1. **Automated Tests**: All automated tests must pass
2. **Code Review**: Code review must be completed
3. **Security Scan**: Security vulnerabilities must be addressed
4. **Performance Tests**: Performance benchmarks must be met
5. **Manual Approval**: QA manual approval required

**Validation Rules**:
- Only **Tester/QA** can validate quality gates
- Blocking gates must pass before deployment
- Required gates must pass (unless skipped)
- At least one gate must be validated by QA before deployment

### Release Workflow Examples

#### Example 1: Standard Release Flow
1. **ProductOwner** creates release → Status: **Planned**
2. **ProductOwner** adds sprints to release → Status: **Planned**
3. **ScrumMaster** starts release preparation → Status: **InProgress**
4. **ScrumMaster** marks ready for testing → Status: **Testing**
5. **Tester** validates quality gates → Quality Gates: **Passed**
6. **Tester** approves release → Status: **ReadyForDeployment**
7. **ProductOwner** deploys release → Status: **Deployed**

#### Example 2: Release with Quality Gate Issues
1. Release reaches **Testing** status
2. **Tester** validates quality gates → Some gates **Failed**
3. **Tester** creates defects for failed gates
4. Team fixes issues → Quality Gates: **Passed**
5. **Tester** approves release → Status: **ReadyForDeployment**
6. **ProductOwner** deploys → Status: **Deployed**

#### Example 3: Cancelled Release
1. Release in **Testing** status
2. Critical security issue discovered
3. **ProductOwner** cancels release → Status: **Cancelled**
4. New release created after fixes

### Release Deployment Rules

1. **QA Approval Required**: Release must be approved by Tester/QA before deployment
2. **Quality Gates Must Pass**: All blocking and required quality gates must pass
3. **QA Validation Required**: At least one quality gate must be validated by a Tester
4. **No Bypass**: ProductOwner cannot deploy without QA approval

---

## Workflow Best Practices

### Task Workflow

1. **Use InReview consistently**: Always move tasks to InReview before Done to ensure quality
2. **Block early**: Mark tasks as Blocked immediately when blockers are discovered
3. **Communicate status**: Update task status regularly to keep team informed
4. **Respect QA approval**: Don't skip review for complex tasks

### Sprint Workflow

1. **Plan before starting**: Ensure all tasks are added before starting sprint
2. **Set clear dates**: Define sprint dates before starting
3. **Close promptly**: Close sprint immediately after completion
4. **One sprint at a time**: Don't start new sprint until current one is closed

### Release Workflow

1. **Early testing**: Move to Testing as soon as features are ready
2. **Validate gates early**: Check quality gates throughout development
3. **Don't skip QA**: Always get QA approval before deployment
4. **Document issues**: Create defects for any quality gate failures

---

## Common Workflow Issues

### Issue 1: "I can't start a sprint"
**Problem**: Sprint start button is disabled.

**Solutions**:
- Verify you have ScrumMaster role
- Ensure sprint has at least one task
- Check that sprint dates are set
- Verify no other sprint is active

### Issue 2: "I can't approve a release"
**Problem**: Release approval button is disabled.

**Solutions**:
- Verify you have Tester/QA role
- Check that quality gates are passed
- Ensure you've validated at least one quality gate
- Check that release is in Testing status

### Issue 3: "I can't deploy a release"
**Problem**: Deploy button is disabled.

**Solutions**:
- Verify you have ProductOwner or ScrumMaster role
- Check that release is in ReadyForDeployment status
- Ensure QA has approved the release
- Verify all blocking quality gates are passed

### Issue 4: "Task stuck in InReview"
**Problem**: Task cannot be moved to Done.

**Solutions**:
- Verify you have Tester, ScrumMaster, or ProductOwner role
- Check that QA approval condition is met
- Ensure task is actually ready (not just in review)

### Issue 5: "Sprint can't be closed"
**Problem**: Sprint completion is blocked.

**Solutions**:
- Verify you have ScrumMaster role
- Check that all tasks are Done
- Ensure no tasks are still InProgress or InReview

---

## Workflow Permissions Summary

### Task Status Changes

| Transition | Developer | Tester | ScrumMaster | ProductOwner |
|-----------|-----------|--------|-------------|--------------|
| Todo → InProgress | ✅ | ✅ | ✅ | ✅ |
| InProgress → InReview | ✅ | ❌ | ✅ | ✅ |
| InReview → Done | ❌ | ✅ | ✅ | ✅ |
| Any → Blocked | ✅ | ✅ | ✅ | ✅ |
| Blocked → Todo/InProgress | ✅ | ✅ | ✅ | ✅ |

### Sprint Status Changes

| Transition | ScrumMaster | ProductOwner |
|-----------|-------------|--------------|
| NotStarted → Active | ✅ | ❌ |
| Active → Completed | ✅ | ❌ |
| Active → Cancelled | ✅ | ✅ |

### Release Status Changes

| Transition | Tester | ScrumMaster | ProductOwner |
|-----------|--------|-------------|-------------|
| Testing → ReadyForDeployment | ✅ | ✅ | ❌ |
| ReadyForDeployment → Deployed | ❌ | ✅ | ✅ |

---

*Last Updated: 2025-01-06*
*Version: 1.0*

