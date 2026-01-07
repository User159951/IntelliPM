# IntelliPM AI Governance Guide

## Overview

IntelliPM includes AI-powered agents that assist with project management tasks. This guide explains how AI decisions work, what requires human approval, who can approve decisions, and how quota management works.

---

## AI Agents Overview

IntelliPM includes several AI agents:

1. **ProductAgent**: Product strategy and backlog prioritization
2. **DeliveryAgent**: Sprint planning and delivery optimization
3. **ManagerAgent**: Project management and resource allocation
4. **QAAgent**: Quality assurance and testing recommendations
5. **BusinessAgent**: Business analysis and stakeholder communication

Each agent can make decisions that may require human approval depending on the decision type and organization policies.

---

## AI Decision Types

### Decision Categories

1. **RiskDetection**: Identifies project risks and suggests mitigation strategies
2. **SprintPlanning**: Recommends sprint scope and task assignments
3. **TaskPrioritization**: Suggests task priority changes
4. **CostDecision**: Financial decisions (e.g., resource allocation)
5. **QuotaDecision**: Quota management decisions
6. **CriticalSystemDecision**: System-critical decisions requiring high-level approval

### Decision Properties

Each AI decision includes:
- **Decision ID**: Unique identifier for tracking
- **Decision Type**: Category of decision
- **Agent Type**: Which agent made the decision
- **Question**: What was asked
- **Decision**: What was decided (JSON or text)
- **Reasoning**: Why this decision (with reasoning chain)
- **Confidence Score**: 0.0 to 1.0 (higher = more confident)
- **Model Information**: Which AI model was used
- **Token Usage**: How many tokens were consumed

---

## Human Approval Requirements

### When Approval is Required

AI decisions require human approval when:

1. **Decision Type Policy**: Organization has configured the decision type to require approval
2. **Confidence Threshold**: Decision confidence is below organization threshold
3. **Critical Decisions**: Decision type is marked as critical (e.g., CriticalSystemDecision)
4. **Cost Threshold**: Decision involves costs above organization threshold
5. **Default Policy**: Organization has enabled approval for all decisions of this type

### When Approval is NOT Required

AI decisions execute automatically when:

1. **Low Risk**: Decision is low-risk and high-confidence
2. **Policy Allows**: Organization policy allows auto-execution for this decision type
3. **Non-Critical**: Decision is not marked as critical
4. **Within Limits**: Decision is within auto-approval limits

---

## Approval Workflow

### Decision Lifecycle

```
Pending → Approved → Applied
    ↓
Rejected
    ↓
Expired
```

### Status Definitions

- **Pending**: Decision is waiting for approval
- **Approved**: Decision has been approved and can be executed
- **Rejected**: Decision has been rejected and will not be executed
- **Applied**: Decision has been executed/applied
- **Expired**: Decision approval deadline has passed

### Approval Process

1. **AI Makes Decision**: Agent generates a decision with `RequiresHumanApproval = true`
2. **Decision Logged**: Decision is stored in `AIDecisionLog` table
3. **Approval Deadline Set**: Default deadline is set (configurable)
4. **Notification Sent**: Approvers are notified (if configured)
5. **Human Reviews**: Authorized user reviews decision
6. **Approval/Rejection**: User approves or rejects with optional notes
7. **Execution**: Approved decisions are executed automatically

### Approval Deadlines

- **Default**: 7 days from decision creation
- **Configurable**: Per organization or decision type
- **Expiration**: Decisions expire if not approved by deadline
- **Extension**: Can be extended by admins (future feature)

---

## Who Can Approve What

### Approval Policies

Approval policies define who can approve specific decision types:

#### Policy Configuration

- **Organization-Level**: Policies can be set per organization
- **Global Policies**: System-wide defaults
- **Decision Type**: Each decision type can have its own policy
- **Required Role**: Policy specifies which role can approve

#### Default Approval Roles

| Decision Type | Default Approver | Can Override |
|---------------|------------------|--------------|
| RiskDetection | ProductOwner | ✅ |
| SprintPlanning | ScrumMaster | ✅ |
| TaskPrioritization | ProductOwner | ✅ |
| CostDecision | Admin | ✅ |
| QuotaDecision | SuperAdmin | ✅ |
| CriticalSystemDecision | SuperAdmin | ❌ |

### Role-Based Approval

#### ProductOwner
- Can approve: RiskDetection, TaskPrioritization, SprintPlanning (if ScrumMaster unavailable)
- Cannot approve: QuotaDecision, CriticalSystemDecision

#### ScrumMaster
- Can approve: SprintPlanning, TaskPrioritization
- Cannot approve: CostDecision, QuotaDecision, CriticalSystemDecision

#### Admin
- Can approve: All decisions within their organization
- Cannot approve: QuotaDecision, CriticalSystemDecision (SuperAdmin only)

#### SuperAdmin
- Can approve: **ALL** decisions across **ALL** organizations
- Exclusive: QuotaDecision, CriticalSystemDecision

### Approval Access Rules

1. **Organization Scope**: Admins can only approve decisions from their organization
2. **SuperAdmin Override**: SuperAdmin can approve decisions from any organization
3. **Role Verification**: System verifies user has required role before allowing approval
4. **Policy Enforcement**: Approval policies are strictly enforced

---

## Quota Management

### Quota Tiers

IntelliPM offers three quota tiers:

#### Free Tier
- **Tokens**: 100,000 per month
- **Requests**: 100 per month
- **Decisions**: 50 per month
- **Cost Limit**: $0 (no overage allowed)
- **Overage**: Not allowed

#### Pro Tier
- **Tokens**: 1,000,000 per month
- **Requests**: 1,000 per month
- **Decisions**: 500 per month
- **Cost Limit**: $100 per month
- **Overage**: Allowed ($0.02 per 1000 tokens)

#### Enterprise Tier
- **Tokens**: 10,000,000 per month
- **Requests**: 10,000 per month
- **Decisions**: 5,000 per month
- **Cost Limit**: $1,000 per month
- **Overage**: Allowed ($0.01 per 1000 tokens)

### Quota Limits

Quotas are tracked per organization and reset monthly:

- **Period**: 30 days (monthly)
- **Reset**: First day of each month (configurable)
- **Tracking**: Real-time usage tracking
- **Alerts**: Warnings at 80% usage (configurable)

### Quota Enforcement

#### When Quota is Enforced

1. **Before AI Request**: System checks quota before processing
2. **Token Limit**: Prevents requests if token limit exceeded
3. **Request Limit**: Prevents requests if request limit exceeded
4. **Decision Limit**: Prevents new decisions if decision limit exceeded
5. **Cost Limit**: Prevents requests if cost limit exceeded

#### Quota Exceeded Behavior

- **Blocking Mode**: Requests are blocked when quota exceeded
- **Non-Blocking Mode**: Requests allowed but tracked (configurable)
- **Overage**: Pro/Enterprise tiers can exceed limits with overage charges
- **Notifications**: Admins are notified when quota is exceeded

### Quota Tracking

#### Usage Metrics

- **Tokens Used**: Total tokens consumed in current period
- **Requests Used**: Total requests made in current period
- **Decisions Made**: Total decisions generated in current period
- **Cost Accumulated**: Total cost in current period (USD)

#### Breakdown by Agent

Usage is tracked per agent type:
- ProductAgent usage
- DeliveryAgent usage
- ManagerAgent usage
- QAAgent usage
- BusinessAgent usage

#### Breakdown by Decision Type

Usage is tracked per decision type:
- RiskDetection usage
- SprintPlanning usage
- TaskPrioritization usage
- etc.

### Quota Management Roles

#### Who Can View Quotas
- **All Users**: Can view their organization's quota status
- **Admins**: Can view detailed quota metrics
- **SuperAdmin**: Can view all organizations' quotas

#### Who Can Manage Quotas
- **Admin**: Can update quota for their organization
- **SuperAdmin**: Can update quota for any organization
- **ProductOwner/ScrumMaster**: Cannot manage quotas (view only)

---

## AI Decision Logging

### Decision Log Structure

All AI decisions are logged with:

- **Decision ID**: Unique identifier (GUID)
- **Organization ID**: Multi-tenant isolation
- **Decision Type**: Category of decision
- **Agent Type**: Which agent made the decision
- **Entity Context**: What entity the decision relates to (Project, Sprint, Task)
- **Question**: What was asked
- **Decision**: What was decided
- **Reasoning**: Why this decision (JSON with reasoning chain)
- **Confidence Score**: 0.0 to 1.0
- **Model Info**: AI model name and version
- **Token Usage**: Prompt tokens, completion tokens, total tokens
- **Input/Output Data**: Full JSON of input context and output
- **Alternatives Considered**: JSON array of alternatives
- **Human Oversight**: Approval status, approver, approval notes
- **Outcome Tracking**: Whether decision was applied, results

### Decision Log Access

- **Users**: Can view decisions related to their projects
- **Admins**: Can view all decisions in their organization
- **SuperAdmin**: Can view all decisions across all organizations

### Decision Log Retention

- **Default**: Decisions are retained indefinitely
- **Configurable**: Per organization retention policies
- **Export**: Decisions can be exported for audit purposes

---

## Approval Policies Configuration

### Setting Approval Policies

Approval policies can be configured at:

1. **Organization Level**: Per-organization policies
2. **Decision Type Level**: Policies for specific decision types
3. **Global Level**: System-wide defaults

### Policy Properties

- **Decision Type**: Which decision type this policy applies to
- **Required Role**: Which role can approve (e.g., "ProductOwner", "Admin")
- **Is Blocking**: Whether approval is required before execution
- **Is Active**: Whether policy is currently active
- **Description**: Optional policy description

### Policy Examples

#### Example 1: Require ProductOwner Approval for Risk Detection
```json
{
  "decisionType": "RiskDetection",
  "requiredRole": "ProductOwner",
  "isBlockingIfNotApproved": true,
  "isActive": true,
  "description": "All risk detection decisions require ProductOwner approval"
}
```

#### Example 2: Allow Auto-Execution for Task Prioritization
```json
{
  "decisionType": "TaskPrioritization",
  "requiredRole": "ProductOwner",
  "isBlockingIfNotApproved": false,
  "isActive": true,
  "description": "Task prioritization can proceed without approval but is tracked"
}
```

#### Example 3: Require SuperAdmin for Quota Decisions
```json
{
  "decisionType": "QuotaDecision",
  "requiredRole": "SuperAdmin",
  "isBlockingIfNotApproved": true,
  "isActive": true,
  "description": "Quota decisions require SuperAdmin approval"
}
```

---

## Best Practices

### For ProductOwners

1. **Review Risk Decisions**: Regularly review risk detection decisions
2. **Approve Promptly**: Approve or reject decisions within deadline
3. **Provide Feedback**: Add notes when approving/rejecting to improve AI
4. **Monitor Quota**: Keep an eye on organization quota usage

### For ScrumMasters

1. **Review Sprint Plans**: Validate AI sprint planning recommendations
2. **Adjust as Needed**: Approve with modifications if needed
3. **Track Decisions**: Monitor how AI decisions affect sprint outcomes

### For Admins

1. **Configure Policies**: Set appropriate approval policies for your organization
2. **Monitor Quota**: Track quota usage and upgrade tier if needed
3. **Review Logs**: Regularly review AI decision logs for patterns
4. **Train Team**: Ensure team understands approval requirements

### For SuperAdmins

1. **System Policies**: Configure global approval policies
2. **Quota Management**: Manage quotas across all organizations
3. **Audit Decisions**: Review critical system decisions
4. **Model Management**: Monitor AI model performance and costs

---

## Common Scenarios

### Scenario 1: AI Recommends Sprint Scope Change
**Situation**: DeliveryAgent recommends adding 3 more tasks to sprint.

**Process**:
1. Decision logged with `RequiresHumanApproval = true`
2. ScrumMaster receives notification
3. ScrumMaster reviews recommendation and reasoning
4. ScrumMaster approves or rejects with notes
5. If approved, sprint scope is updated automatically

**Who Can Approve**: ScrumMaster (or ProductOwner if ScrumMaster unavailable)

### Scenario 2: AI Detects Project Risk
**Situation**: ManagerAgent detects that sprint velocity is declining.

**Process**:
1. Risk detection decision logged
2. ProductOwner receives notification
3. ProductOwner reviews risk details and mitigation suggestions
4. ProductOwner approves to apply mitigation or rejects if false positive
5. If approved, risk mitigation actions are executed

**Who Can Approve**: ProductOwner (or Admin)

### Scenario 3: Quota Exceeded
**Situation**: Organization has used all tokens for the month.

**Process**:
1. Next AI request checks quota
2. Quota exceeded → Request blocked
3. Admin receives notification
4. Admin can:
   - Upgrade tier (if allowed)
   - Wait for quota reset
   - Request quota increase (SuperAdmin)

**Who Can Resolve**: Admin (upgrade) or SuperAdmin (increase quota)

### Scenario 4: Critical System Decision
**Situation**: AI recommends system-wide configuration change.

**Process**:
1. Decision logged as `CriticalSystemDecision`
2. Requires SuperAdmin approval (cannot be delegated)
3. SuperAdmin reviews decision
4. SuperAdmin approves or rejects
5. If approved, system configuration is updated

**Who Can Approve**: SuperAdmin **ONLY**

---

## Troubleshooting

### "Decision Approval Failed"
**Problem**: Cannot approve a decision.

**Solutions**:
- Verify you have the required role (check policy)
- Check that decision hasn't expired
- Ensure decision is in Pending status
- Verify you have access to the organization

### "Quota Exceeded"
**Problem**: AI requests are blocked due to quota.

**Solutions**:
- Check current quota usage in Admin panel
- Upgrade to higher tier if needed
- Wait for monthly quota reset
- Contact SuperAdmin for quota increase

### "Approval Deadline Passed"
**Problem**: Decision expired before approval.

**Solutions**:
- Decision cannot be approved after expiration
- Review decision log to understand what was decided
- New decision will be generated if needed
- Contact SuperAdmin to extend deadline (future feature)

### "Cannot View Decisions"
**Problem**: Cannot see AI decisions.

**Solutions**:
- Verify you have access to the project/organization
- Check that decisions exist for your projects
- Ensure you're viewing the correct organization
- Contact Admin if access issues persist

---

## Security Considerations

### Multi-Tenancy

- **Organization Isolation**: Decisions are isolated per organization
- **Access Control**: Users can only view decisions from their organization
- **SuperAdmin Override**: SuperAdmin can access all organizations

### Audit Trail

- **Complete Logging**: All decisions are logged with full context
- **Approval Tracking**: Who approved/rejected and when
- **Execution Tracking**: Whether decision was applied and results
- **Export Capability**: Decisions can be exported for compliance

### Data Privacy

- **No PII in Logs**: Personal information is not included in decision logs
- **Secure Storage**: Decision logs are stored securely
- **Retention Policies**: Configurable retention periods
- **GDPR Compliance**: Decisions can be deleted on request

---

*Last Updated: 2025-01-06*
*Version: 1.0*

