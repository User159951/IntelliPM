# IntelliPM Backend Documentation

**Version:** 2.14.4  
**Last Updated:** January 6, 2025  
**Technology Stack:** .NET 8.0, ASP.NET Core, Entity Framework Core, SQL Server, PostgreSQL, Semantic Kernel

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture](#architecture)
3. [Project Structure](#project-structure)
4. [Domain Layer](#domain-layer)
5. [Application Layer](#application-layer)
6. [Infrastructure Layer](#infrastructure-layer)
7. [API Layer](#api-layer)
8. [Security](#security)
9. [Database](#database)
10. [Configuration](#configuration)
11. [Development Setup](#development-setup)
12. [Testing](#testing)
13. [Deployment](#deployment)
14. [API Reference](#api-reference)
15. [Troubleshooting](#troubleshooting)
16. [Best Practices](#best-practices)
17. [Future Improvements](#future-improvements)
18. [Contributing](#contributing)
19. [Support](#support)
20. [Missing Features](#missing-features)
21. [API Endpoints Audit](#api-endpoints-audit)

---

## 1. Overview

### 1.1 Introduction

IntelliPM is an intelligent project management system that combines traditional project management features with AI-powered agents for automated insights, risk detection, and sprint planning. The backend is built using Clean Architecture principles, ensuring maintainability, testability, and scalability.

### 1.2 Key Features

- **Multi-Tenant Architecture**: Organization-based isolation
- **Role-Based Access Control (RBAC)**: Global and project-level permissions
- **CQRS Pattern**: Separation of commands and queries
- **AI Agents**: Semantic Kernel-powered agents for project analysis
- **Real-time Notifications**: Activity tracking and alerts
- **Comprehensive Metrics**: Velocity, burndown, defect tracking
- **Vector Store**: PostgreSQL with pgvector for AI agent memory
- **Domain Events**: Event-driven architecture with domain events infrastructure
- **Outbox Pattern**: Reliable event publishing with retry logic and idempotency
- **Feature Flags**: Dynamic feature toggle system with organization-level control
- **Structured Logging**: Serilog with Console, File, and Seq sinks
- **Error Tracking**: Sentry integration with performance monitoring
- **Comment System**: Polymorphic comments with threading support
- **Mention System**: @username mentions with notification tracking
- **Notification Preferences**: User-configurable notification settings
- **File Attachments**: File upload/download with validation
- **AI Governance**: Decision logging, quota management, and kill switch
- **Read Models**: CQRS read models for optimized queries (TaskBoard, SprintSummary, ProjectOverview)
- **Organization Permission Policies**: Per-organization permission restrictions managed by SuperAdmin
- **Member Permission Management**: Admin-level member role and permission management with policy enforcement

### 1.3 Technology Stack

| Component | Technology |
|-----------|-----------|
| **Framework** | .NET 8.0 |
| **Web Framework** | ASP.NET Core Web API |
| **ORM** | Entity Framework Core 8.0 |
| **Primary Database** | SQL Server (Transactional data) |
| **Vector Database** | PostgreSQL with pgvector (AI memory) |
| **Authentication** | JWT Bearer Tokens |
| **Caching** | In-Memory Cache |
| **Logging** | Serilog (Console, File, Seq) |
| **Monitoring** | Sentry (Error tracking & performance) |
| **AI Framework** | Microsoft Semantic Kernel |
| **LLM** | Ollama (Local LLM) |
| **Email** | SMTP |
| **Validation** | FluentValidation |
| **Mediator** | MediatR |
| **Background Services** | .NET BackgroundService (Outbox processor) |

---

## 2. Architecture

### 2.1 Clean Architecture

The backend follows Clean Architecture principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                    API Layer                             │
│  (Controllers, Middleware, Filters, Authorization)      │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│              Application Layer                           │
│  (Commands, Queries, Handlers, DTOs, Behaviors)         │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                 Domain Layer                             │
│  (Entities, Value Objects, Enums, Interfaces)           │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│            Infrastructure Layer                          │
│  (Repositories, Services, Database, External APIs)      │
└─────────────────────────────────────────────────────────┘
```

### 2.2 Dependency Flow

- **Domain**: No dependencies (pure business logic)
- **Application**: Depends on Domain only
- **Infrastructure**: Depends on Application and Domain
- **API**: Depends on Application, Infrastructure, and Domain

### 2.3 Design Patterns

- **CQRS (Command Query Responsibility Segregation)**: Separate read and write operations
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Mediator Pattern**: Decoupled request handling via MediatR
- **Strategy Pattern**: Multiple AI agent implementations
- **Factory Pattern**: DbContext factories
- **Outbox Pattern**: Reliable event publishing with idempotency and retry logic
- **Feature Toggle Pattern**: Dynamic feature flag management

---

## 3. Project Structure

### 3.1 Solution Structure

```
IntelliPM.sln
├── IntelliPM.Domain/              # Domain layer (business entities)
├── IntelliPM.Application/         # Application layer (use cases)
├── IntelliPM.Infrastructure/      # Infrastructure layer (data, external services)
├── IntelliPM.API/                 # API layer (controllers, middleware)
└── IntelliPM.Tests/               # Test projects
```

### 3.2 Domain Layer Structure

```
IntelliPM.Domain/
├── Entities/                      # Domain entities (44 entities)
│   ├── Project.cs
│   ├── User.cs
│   ├── ProjectTask.cs
│   ├── Sprint.cs
│   ├── OutboxMessage.cs          # Outbox pattern entity
│   ├── DeadLetterMessage.cs      # Dead Letter Queue entity
│   ├── FeatureFlag.cs            # Feature flag entity
│   ├── OrganizationInvitation.cs # Organization invitation entity
│   ├── Organization.cs           # Organization entity
│   ├── Team.cs                   # Team entity
│   ├── ProjectTeam.cs            # Project-Team relationship entity
│   ├── Defect.cs                 # Defect entity
│   ├── Alert.cs                  # Alert entity
│   ├── Notification.cs           # Notification entity
│   ├── Invitation.cs             # Project invitation entity
│   ├── BacklogItem.cs            # Backlog item entity
│   ├── Insight.cs                # Insight entity
│   ├── Risk.cs                   # Risk entity
│   ├── Permission.cs             # Permission entity
│   ├── RolePermission.cs         # Role permission mapping
│   ├── GlobalSetting.cs          # Global setting entity
│   ├── Activity.cs               # Activity feed entity
│   ├── AuditLog.cs               # Audit log entity
│   ├── AgentExecutionLog.cs      # AI agent execution log
│   ├── DocumentStore.cs          # Document store entity
│   ├── PasswordResetToken.cs      # Password reset token entity
│   ├── ProjectMember.cs          # Project member relationship
│   ├── Comment.cs                # Comment entity (polymorphic)
│   ├── Mention.cs                # Mention entity for @username tracking
│   ├── NotificationPreference.cs # User notification preferences
│   ├── Attachment.cs             # File attachment entity
│   ├── AIDecisionLog.cs          # AI decision logging entity
│   ├── AIQuota.cs                # AI quota and usage tracking entity
│   ├── TaskBoardReadModel.cs     # Read model for task board
│   ├── SprintSummaryReadModel.cs # Read model for sprint summary
│   ├── ProjectOverviewReadModel.cs # Read model for project overview
│   ├── Milestone.cs              # Milestone entity
│   ├── Release.cs                # Release entity
│   ├── QualityGate.cs            # Quality gate entity
│   ├── TaskDependency.cs         # Task dependency entity
│   └── OrganizationPermissionPolicy.cs # Organization permission policy entity
├── Events/                        # Domain events (23 events)
│   ├── IDomainEvent.cs           # Domain event interface
│   ├── CommentAddedEvent.cs      # Event when comment is added
│   ├── CommentUpdatedEvent.cs    # Event when comment is updated
│   ├── CommentDeletedEvent.cs    # Event when comment is deleted
│   ├── UserMentionedEvent.cs     # Event when user is mentioned
│   ├── ProjectCreatedEvent.cs    # Event when project is created
│   ├── ProjectUpdatedEvent.cs    # Event when project is updated
│   ├── TaskCreatedEvent.cs       # Event when task is created
│   ├── TaskUpdatedEvent.cs       # Event when task is updated
│   ├── TaskDeletedEvent.cs       # Event when task is deleted
│   ├── SprintCreatedEvent.cs     # Event when sprint is created
│   ├── SprintUpdatedEvent.cs     # Event when sprint is updated
│   ├── SprintStartedEvent.cs     # Event when sprint is started
│   ├── SprintCompletedEvent.cs   # Event when sprint is completed
│   ├── UserCreatedEvent.cs       # Event when user is created
│   ├── UserUpdatedEvent.cs       # Event when user is updated
│   ├── MemberAddedToProjectEvent.cs    # Event when member is added to project
│   ├── MemberRemovedFromProjectEvent.cs # Event when member is removed from project
│   ├── MilestoneCreatedEvent.cs  # Event when milestone is created
│   ├── MilestoneCompletedEvent.cs # Event when milestone is completed
│   ├── MilestoneMissedEvent.cs   # Event when milestone is missed
│   ├── ReleaseNotesGeneratedEvent.cs # Event when release notes are generated
│   └── QualityGatesEvaluatedEvent.cs # Event when quality gates are evaluated
├── ValueObjects/                  # Value objects
│   └── StoryPoints.cs
├── Enums/                         # Enumerations
│   ├── GlobalRole.cs
│   ├── ProjectRole.cs
│   └── UserRole.cs
├── Constants/                     # Domain constants (10 files)
│   ├── ProjectConstants.cs
│   ├── TaskConstants.cs
│   ├── SprintConstants.cs
│   ├── TeamConstants.cs
│   ├── NotificationConstants.cs  # Notification types and frequencies
│   ├── AttachmentConstants.cs    # File attachment limits and types
│   ├── AIDecisionConstants.cs     # AI decision types and statuses
│   ├── AIQuotaConstants.cs       # AI quota tiers and limits
│   ├── CommentConstants.cs       # Comment-related constants
│   └── OrganizationConstants.cs  # Organization-related constants
└── Interfaces/                    # Domain interfaces
    └── IAggregateRoot.cs
```

### 3.3 Application Layer Structure

```
IntelliPM.Application/
├── [Feature]/                     # Feature-based organization
│   ├── Commands/                  # Write operations
│   │   ├── [Feature]Command.cs
│   │   ├── [Feature]CommandHandler.cs
│   │   └── [Feature]CommandValidator.cs
│   └── Queries/                   # Read operations
│       ├── [Feature]Query.cs
│       └── [Feature]QueryHandler.cs
├── Admin/                         # Admin commands
│   └── Commands/                  # Organization management
│       ├── InviteOrganizationUserCommand.cs
│       ├── InviteOrganizationUserCommandValidator.cs
│       └── Handlers/
│           └── InviteOrganizationUserCommandHandler.cs
├── Identity/                      # Authentication & user management
│   ├── Commands/
│   │   ├── AcceptOrganizationInviteCommand.cs
│   │   ├── AcceptOrganizationInviteCommandHandler.cs
│   │   ├── AcceptOrganizationInviteCommandValidator.cs
│   │   └── ...
│   └── Queries/
├── FeatureFlags/                  # Feature flag management
│   ├── Commands/                  # Create, Update commands
│   └── Queries/                   # GetAll query with DTOs
├── Common/                        # Shared application code
│   ├── Authorization/             # Permission classes
│   ├── Behaviors/                 # MediatR behaviors
│   ├── Exceptions/                # Custom exceptions
│   ├── Interfaces/                # Application interfaces
│   │   ├── ICurrentUserService.cs
│   │   ├── IDomainEventDispatcher.cs
│   │   ├── IFeatureFlagService.cs
│   │   ├── IEmailService.cs
│   │   └── ...
│   ├── Models/                    # Common models
│   └── Services/                  # Application services
│       └── DomainEventDispatcher.cs
├── Comments/                      # Comment management
│   ├── Commands/
│   │   ├── AddCommentCommand.cs
│   │   ├── AddCommentCommandHandler.cs
│   │   └── AddCommentCommandValidator.cs
│   └── Queries/
├── AI/                            # AI governance
│   ├── Commands/
│   │   ├── UpdateAIQuotaCommand.cs
│   │   ├── DisableAIForOrgCommand.cs
│   │   ├── EnableAIForOrgCommand.cs
│   │   ├── ApproveAIDecisionCommand.cs
│   │   └── RejectAIDecisionCommand.cs
│   └── Queries/
│       ├── GetAIDecisionLogsQuery.cs
│       ├── GetAIQuotaStatusQuery.cs
│       ├── GetAIUsageStatisticsQuery.cs
│       ├── GetAllAIQuotasQuery.cs
│       ├── GetAllAIDecisionLogsQuery.cs
│       └── ExportAIDecisionsQuery.cs
├── Notifications/                  # Notification handling
│   └── Handlers/
│       └── UserMentionedEventHandler.cs
├── Services/                      # Application services
│   ├── MentionParser.cs           # Parses @username mentions
│   ├── NotificationPreferenceService.cs # Manages notification preferences
│   ├── FileStorageService.cs      # File storage operations
│   └── AIAvailabilityService.cs  # Checks AI availability for orgs
├── DTOs/                          # Data Transfer Objects
└── DependencyInjection.cs         # Service registration
```

### 3.4 Infrastructure Layer Structure

```
IntelliPM.Infrastructure/
├── Persistence/                   # Database access
│   ├── AppDbContext.cs           # SQL Server context
│   ├── VectorDbContext.cs        # PostgreSQL context
│   ├── GenericRepository.cs      # Repository implementation
│   ├── UnitOfWork.cs             # Unit of Work implementation
│   ├── Configurations/           # Entity configurations (Fluent API)
│   │   ├── OutboxMessageConfiguration.cs
│   │   ├── FeatureFlagConfiguration.cs
│   │   └── ...
│   └── Migrations/               # EF Core migrations
├── BackgroundServices/           # Background services
│   └── OutboxProcessor.cs        # Outbox pattern processor
├── Identity/                      # Authentication & authorization
│   ├── AuthService.cs
│   ├── JwtTokenService.cs
│   └── PasswordHasher.cs
├── Services/                      # Infrastructure services
│   ├── EmailService.cs
│   ├── PermissionService.cs
│   ├── CacheService.cs
│   ├── CurrentUserService.cs
│   └── FeatureFlagService.cs     # Feature flag with caching
├── AI/                            # AI agent implementations
│   ├── Handlers/                  # Command handlers for AI
│   ├── Plugins/                   # Semantic Kernel plugins
│   └── Services/                  # Agent services
├── VectorStore/                   # Vector database implementation
├── LLM/                           # LLM client (Ollama)
└── Health/                        # Health checks
```

### 3.5 API Layer Structure

```
IntelliPM.API/
├── Controllers/                   # API controllers (42 controllers total: 26 standard + 14 admin + 2 superadmin + 1 DEBUG-only TestController)
│   ├── BaseApiController.cs      # Base controller
│   ├── ProjectsController.cs
│   ├── TasksController.cs
│   ├── SprintsController.cs
│   ├── TeamsController.cs
│   ├── DefectsController.cs
│   ├── MilestonesController.cs
│   ├── ReleasesController.cs
│   ├── BacklogController.cs
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── NotificationsController.cs
│   ├── MetricsController.cs
│   ├── AgentsController.cs
│   ├── AgentController.cs
│   ├── SearchController.cs
│   ├── SettingsController.cs
│   ├── PermissionsController.cs
│   ├── AlertsController.cs
│   ├── ActivityController.cs
│   ├── InsightsController.cs
│   ├── FeatureFlagsController.cs
│   ├── HealthController.cs
│   ├── HealthApiController.cs    # API smoke tests (no versioning)
│   ├── TestController.cs         # DEBUG-only: Conditioned with #if DEBUG
│   ├── ~~AdminHashGeneratorController.cs~~ (REMOVED - Security vulnerability)
│   ├── Admin/                     # Admin controllers (14 controllers)
│   │   ├── UsersController.cs
│   │   ├── FeatureFlagsController.cs
│   │   ├── DashboardController.cs
│   │   ├── AuditLogsController.cs
│   │   ├── SystemHealthController.cs
│   │   ├── DeadLetterQueueController.cs
│   │   ├── ReadModelsController.cs
│   │   ├── AIGovernanceController.cs
│   │   ├── AdminMemberPermissionsController.cs
│   │   ├── AdminAIQuotaController.cs
│   │   ├── OrganizationsController.cs
│   │   └── OrganizationController.cs
│   └── SuperAdmin/                # SuperAdmin controllers (2 controllers)
│       ├── SuperAdminPermissionPolicyController.cs
│       └── SuperAdminAIQuotaController.cs
│   └── ...
├── Authorization/                 # Authorization attributes
│   └── RequirePermissionAttribute.cs
├── Middleware/                    # Custom middleware
│   ├── SentryUserContextMiddleware.cs
│   └── FeatureFlagMiddleware.cs  # Feature flag checking
├── Program.cs                     # Application entry point
└── appsettings.json               # Configuration files
```

---

## 4. Domain Layer

### 4.1 Core Entities

#### 4.1.1 Project Entity

```csharp
public class Project : IAggregateRoot
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }              // Scrum, Kanban, etc.
    public int SprintDurationDays { get; set; }
    public int OwnerId { get; set; }
    public int OrganizationId { get; set; }       // Multi-tenancy
    public string Status { get; set; }            // Active, Archived, etc.
    public byte[] RowVersion { get; set; }       // Optimistic concurrency
    
    // Navigation properties
    public User Owner { get; set; }
    public Organization Organization { get; set; }
    public ICollection<ProjectMember> Members { get; set; }
    public ICollection<Sprint> Sprints { get; set; }
    // ... other collections
}
```

#### 4.1.2 User Entity

```csharp
public class User : IAggregateRoot
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
    public GlobalRole GlobalRole { get; set; }    // Admin, User
    public int OrganizationId { get; set; }      // Multi-tenancy
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }  // Tracks last login timestamp
    
    // Navigation properties
    public Organization Organization { get; set; }
    public ICollection<ProjectMember> ProjectMembers { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; }
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; }
}
```

#### 4.1.3 ProjectTask Entity

```csharp
public class ProjectTask : IAggregateRoot
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int OrganizationId { get; set; }      // Multi-tenancy
    public string Title { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }           // Todo, InProgress, Done, etc.
    public string Priority { get; set; }        // Low, Medium, High, Critical
    public StoryPoints? StoryPoints { get; set; } // Value object
    public int? AssigneeId { get; set; }
    public int? SprintId { get; set; }
    
    // Navigation properties
    public Project Project { get; set; }
    public User? Assignee { get; set; }
    public Sprint? Sprint { get; set; }
}
```

#### 4.1.4 Comment Entity

```csharp
public class Comment : IAggregateRoot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; } // Multi-tenancy
    
    // Polymorphic relationship
    public string EntityType { get; set; } = string.Empty; // "Task", "Project", "Sprint", "Defect"
    public int EntityId { get; set; }
    
    // Comment content
    public string Content { get; set; } = string.Empty; // Rich text or markdown
    
    // Author information
    public int AuthorId { get; set; }
    
    // Metadata
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; } // Soft delete
    public bool IsEdited { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    
    // Parent comment for threading (optional)
    public int? ParentCommentId { get; set; }
    
    // Navigation properties
    public User Author { get; set; } = null!;
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<Mention> Mentions { get; set; } = new List<Mention>();
}
```

#### 4.1.5 Mention Entity

```csharp
public class Mention : IAggregateRoot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; } // Multi-tenancy
    
    // Comment reference
    public int CommentId { get; set; }
    
    // Mentioned user
    public int MentionedUserId { get; set; }
    
    // Mention metadata
    public int StartIndex { get; set; } // Position in comment content
    public int Length { get; set; } // Length of mention text (@username)
    public string MentionText { get; set; } = string.Empty; // e.g., "@john.doe"
    
    // Notification tracking
    public bool NotificationSent { get; set; } = false;
    public DateTimeOffset? NotificationSentAt { get; set; }
    
    // Metadata
    public DateTimeOffset CreatedAt { get; set; }
    
    // Navigation properties
    public Comment Comment { get; set; } = null!;
    public User MentionedUser { get; set; } = null!;
}
```

#### 4.1.6 NotificationPreference Entity

```csharp
public class NotificationPreference : IAggregateRoot
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int OrganizationId { get; set; } // Multi-tenancy
    
    // Notification type
    public string NotificationType { get; set; } = string.Empty; // "TaskAssigned", "Mention", etc.
    
    // Channel preferences
    public bool EmailEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
    public bool PushEnabled { get; set; } = false; // Future: push notifications
    
    // Frequency settings
    public string Frequency { get; set; } = "Instant"; // "Instant", "Daily", "Weekly", "Never"
    
    // Metadata
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}
```

#### 4.1.7 Attachment Entity

```csharp
public class Attachment : IAggregateRoot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; } // Multi-tenancy
    
    // Polymorphic relationship
    public string EntityType { get; set; } = string.Empty; // "Task", "Project", "Comment", etc.
    public int EntityId { get; set; }
    
    // File information
    public string FileName { get; set; } = string.Empty; // Original filename
    public string StoredFileName { get; set; } = string.Empty; // Unique filename on disk
    public string FileExtension { get; set; } = string.Empty; // .pdf, .png, etc.
    public string ContentType { get; set; } = string.Empty; // MIME type
    public long FileSizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty; // Relative path or cloud URL
    
    // Upload information
    public int UploadedById { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    
    // Soft delete
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }
    
    // Navigation properties
    public User UploadedBy { get; set; } = null!;
}
```

#### 4.1.8 AIDecisionLog Entity

```csharp
public class AIDecisionLog : IAggregateRoot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; } // Multi-tenancy
    
    // Decision identification
    public Guid DecisionId { get; set; } = Guid.NewGuid(); // Unique decision identifier
    public string DecisionType { get; set; } = string.Empty; // "RiskDetection", "SprintPlanning", etc.
    public string AgentType { get; set; } = string.Empty; // "DeliveryAgent", "ProductAgent", etc.
    
    // Context
    public string EntityType { get; set; } = string.Empty; // "Project", "Sprint", "Task"
    public int EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty; // Denormalized for quick reference
    
    // Decision details
    public string Question { get; set; } = string.Empty; // What was asked
    public string Decision { get; set; } = string.Empty; // What was decided (JSON or text)
    public string Reasoning { get; set; } = string.Empty; // Why this decision (JSON with reasoning chain)
    public decimal ConfidenceScore { get; set; } // 0.0 to 1.0
    
    // AI Model information
    public string ModelName { get; set; } = string.Empty; // "llama3.2:3b", "gpt-4", etc.
    public string ModelVersion { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    
    // Input/Output
    public string InputData { get; set; } = string.Empty; // JSON of input context
    public string OutputData { get; set; } = string.Empty; // JSON of full output
    public string AlternativesConsidered { get; set; } = "[]"; // JSON array of alternatives
    
    // Human oversight
    public int RequestedByUserId { get; set; }
    public bool RequiresHumanApproval { get; set; } = false;
    public bool? ApprovedByHuman { get; set; } // null = pending, true = approved, false = rejected
    public int? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }
    
    // Outcome tracking
    public string Status { get; set; } = "Pending"; // "Pending", "Applied", "Rejected", "Overridden"
    public bool WasApplied { get; set; } = false;
    public DateTimeOffset? AppliedAt { get; set; }
    public string? ActualOutcome { get; set; } // What actually happened after decision
    
    // Metadata
    public DateTimeOffset CreatedAt { get; set; }
    public int ExecutionTimeMs { get; set; } // How long decision took
    public string? ErrorMessage { get; set; }
    public bool IsSuccess { get; set; } = true;
    
    // Navigation properties
    public User RequestedByUser { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
    
    // Helper methods for JSON serialization
    public List<AlternativeDecision> GetAlternativesConsidered();
    public void SetAlternativesConsidered(List<AlternativeDecision> alternatives);
    public T GetInputDataAs<T>() where T : class;
    public void SetInputData<T>(T data) where T : class;
    public T GetOutputDataAs<T>() where T : class;
    public void SetOutputData<T>(T data) where T : class;
    public void ApproveDecision(int approvedByUserId, string? notes = null);
    public void RejectDecision(int rejectedByUserId, string? notes = null);
}
```

#### 4.1.9 OrganizationPermissionPolicy Entity

```csharp
public class OrganizationPermissionPolicy : IAggregateRoot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; } // Unique - one policy per organization
    public string AllowedPermissionsJson { get; set; } = "[]"; // JSON array of permission names
    public bool IsActive { get; set; } = true; // If false, all permissions allowed
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Navigation properties
    public Organization Organization { get; set; } = null!;
    
    // Helper methods
    public List<string> GetAllowedPermissions();
    public void SetAllowedPermissions(List<string> permissions);
    public bool IsPermissionAllowed(string permission);
}
```

**Key Features:**
- **One Policy Per Organization**: Unique constraint on `OrganizationId`
- **JSON Storage**: Permissions stored as JSON array for flexibility
- **Default Behavior**: If no policy exists or policy is inactive, all permissions are allowed
- **Permission Checking**: `IsPermissionAllowed()` method checks if a permission is in the allowed list
- **Case-Insensitive**: Permission name matching is case-insensitive

**Default Behavior:**
- If `OrganizationPermissionPolicy` doesn't exist for an organization → all permissions allowed
- If `IsActive = false` → all permissions allowed
- If `AllowedPermissionsJson` is empty → all permissions allowed
- If `IsActive = true` and permissions are specified → only listed permissions allowed

#### 4.1.10 AIQuota Entity

```csharp
public class AIQuota : IAggregateRoot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string TierName { get; set; } = "Free"; // "Free", "Pro", "Enterprise", "Custom", "Disabled"
    public bool IsActive { get; set; } = true;
    
    // Period
    public DateTimeOffset PeriodStartDate { get; set; }
    public DateTimeOffset PeriodEndDate { get; set; }
    
    // Limits
    public int MaxTokensPerPeriod { get; set; }
    public int MaxRequestsPerPeriod { get; set; }
    public int MaxDecisionsPerPeriod { get; set; }
    public decimal MaxCostPerPeriod { get; set; }
    
    // Usage tracking
    public int TokensUsed { get; set; } = 0;
    public int RequestsUsed { get; set; } = 0;
    public int DecisionsMade { get; set; } = 0;
    public decimal CostAccumulated { get; set; } = 0m;
    public string UsageByAgentJson { get; set; } = "{}"; // JSON breakdown by agent
    public string UsageByDecisionTypeJson { get; set; } = "{}"; // JSON breakdown by decision type
    
    // Quota enforcement
    public bool EnforceQuota { get; set; } = true;
    public bool IsQuotaExceeded { get; set; } = false;
    public DateTimeOffset? QuotaExceededAt { get; set; }
    public string? QuotaExceededReason { get; set; }
    
    // Alerts
    public decimal AlertThresholdPercentage { get; set; } = 80m;
    public bool AlertSent { get; set; } = false;
    public DateTimeOffset? AlertSentAt { get; set; }
    
    // Overage
    public bool AllowOverage { get; set; } = false;
    public decimal OverageRate { get; set; } = 0m;
    public int OverageTokensUsed { get; set; } = 0;
    public decimal OverageCost { get; set; } = 0m;
    
    // Metadata
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? LastResetAt { get; set; }
    // Legacy field - kept for database compatibility (not used)
    public string? BillingReferenceId { get; set; }
    public bool IsPaid { get; set; } = true;
    
    // Navigation properties
    public Organization Organization { get; set; } = null!;
    
    // Helper methods
    public Dictionary<string, AgentUsage> GetUsageByAgent();
    public void SetUsageByAgent(Dictionary<string, AgentUsage> usage);
    public Dictionary<string, DecisionTypeUsage> GetUsageByDecisionType();
    public void SetUsageByDecisionType(Dictionary<string, DecisionTypeUsage> usage);
    public void RecordUsage(int tokens, string agentType, string decisionType, decimal cost);
    public void CheckQuotaExceeded();
    public bool ShouldSendAlert();
    public void MarkAlertSent();
    public void ResetQuota();
    public QuotaStatus GetQuotaStatus();
}
```

### 4.2 Value Objects

#### 4.2.1 StoryPoints

```csharp
public class StoryPoints
{
    public int Value { get; }
    
    public StoryPoints(int value)
    {
        if (value < 0)
            throw new ArgumentException("Story points cannot be negative");
        Value = value;
    }
}
```

### 4.3 Enumerations

#### 4.3.1 GlobalRole

```csharp
public enum GlobalRole
{
    User = 1,
    Admin = 2
}
```

#### 4.3.2 ProjectRole

```csharp
public enum ProjectRole
{
    ProductOwner,
    ScrumMaster,
    Developer,
    Tester,
    Viewer
}
```

### 4.4 Aggregate Roots

Entities implementing `IAggregateRoot`:
- Project
- User
- ProjectTask
- Sprint
- Team
- ProjectTeam
- Defect
- Alert
- Notification
- Invitation
- OrganizationInvitation
- BacklogItem (Epic, Feature, UserStory)
- Insight
- Risk
- Permission
- RolePermission
- GlobalSetting
- Activity
- FeatureFlag
- OutboxMessage
- DeadLetterMessage
- Organization
- AuditLog
- AgentExecutionLog
- DocumentStore
- PasswordResetToken
- Comment
- Mention
- NotificationPreference
- Attachment
- AIDecisionLog
- AIQuota
- TaskBoardReadModel
- SprintSummaryReadModel
- ProjectOverviewReadModel
- Milestone
- Release
- QualityGate
- TaskDependency
- OrganizationPermissionPolicy

### 4.5 Domain Events

#### 4.5.1 IDomainEvent Interface

```csharp
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
```

Domain events are used to trigger side effects and maintain eventual consistency across the system.

#### 4.5.2 Comment Domain Events

**CommentAddedEvent:**
```csharp
public record CommentAddedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int CommentId { get; init; }
    public int AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public int EntityId { get; init; }
    public string Content { get; init; } = string.Empty;
    public int? ParentCommentId { get; init; }
    public int OrganizationId { get; init; }
}
```

**CommentUpdatedEvent:**
```csharp
public record CommentUpdatedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int CommentId { get; init; }
    public int AuthorId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public int EntityId { get; init; }
    public string OldContent { get; init; } = string.Empty;
    public string NewContent { get; init; } = string.Empty;
    public int OrganizationId { get; init; }
}
```

**CommentDeletedEvent:**
```csharp
public record CommentDeletedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int CommentId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public int EntityId { get; init; }
    public int OrganizationId { get; init; }
}
```

**UserMentionedEvent:**
```csharp
public record UserMentionedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int MentionId { get; init; }
    public int MentionedUserId { get; init; }
    public int CommentId { get; init; }
    public int CommentAuthorId { get; init; }
    public string CommentAuthorName { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public int EntityId { get; init; }
    public string EntityTitle { get; init; } = string.Empty;
    public string MentionText { get; init; } = string.Empty;
    public string CommentContent { get; init; } = string.Empty;
    public int OrganizationId { get; init; }
}
```

**ProjectCreatedEvent:**
```csharp
public record ProjectCreatedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int ProjectId { get; init; }
    public int OrganizationId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
    public string ProjectType { get; init; } = string.Empty;
    public int OwnerId { get; init; }
    public string Status { get; init; } = string.Empty;
}
```

**ProjectUpdatedEvent:**
```csharp
public record ProjectUpdatedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int ProjectId { get; init; }
    public int OrganizationId { get; init; }
    public string ProjectName { get; init; } = string.Empty;
}
```

**TaskCreatedEvent:**
```csharp
public record TaskCreatedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int TaskId { get; init; }
    public int ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public int? StoryPoints { get; init; }
    public int? SprintId { get; init; }
}
```

**TaskUpdatedEvent:**
```csharp
public record TaskUpdatedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int TaskId { get; init; }
    public int ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
}
```

**TaskDeletedEvent:**
```csharp
public record TaskDeletedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int TaskId { get; init; }
    public int ProjectId { get; init; }
}
```

**SprintCreatedEvent:**
```csharp
public record SprintCreatedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int SprintId { get; init; }
    public int ProjectId { get; init; }
    public int OrganizationId { get; init; }
    public int Number { get; init; }
    public string Goal { get; init; } = string.Empty;
}
```

**SprintUpdatedEvent:**
```csharp
public record SprintUpdatedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int SprintId { get; init; }
    public int ProjectId { get; init; }
    public int OrganizationId { get; init; }
}
```

**SprintStartedEvent:**
```csharp
public record SprintStartedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int SprintId { get; init; }
    public int ProjectId { get; init; }
    public int OrganizationId { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
}
```

**SprintCompletedEvent:**
```csharp
public record SprintCompletedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int SprintId { get; init; }
    public int ProjectId { get; init; }
    public int OrganizationId { get; init; }
    public DateTimeOffset CompletedAt { get; init; }
}
```

**UserCreatedEvent:**
```csharp
public record UserCreatedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public GlobalRole Role { get; init; }
    public int OrganizationId { get; init; }
    public int? CreatedById { get; init; }
}
```

**UserUpdatedEvent:**
```csharp
public record UserUpdatedEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public GlobalRole Role { get; init; }
    public int OrganizationId { get; init; }
    public int? UpdatedById { get; init; }
}
```

**MemberAddedToProjectEvent:**
```csharp
public record MemberAddedToProjectEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int ProjectId { get; init; }
    public int UserId { get; init; }
    public ProjectRole Role { get; init; }
    public int OrganizationId { get; init; }
}
```

**MemberRemovedFromProjectEvent:**
```csharp
public record MemberRemovedFromProjectEvent : IDomainEvent, INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public int ProjectId { get; init; }
    public int UserId { get; init; }
    public int OrganizationId { get; init; }
}
```

### 4.6 OutboxMessage Entity

The OutboxMessage entity implements the Outbox pattern for reliable event publishing:

```csharp
public class OutboxMessage
{
    public Guid Id { get; private set; }
    public string EventType { get; private set; }      // Fully qualified type name
    public string Payload { get; private set; }        // JSON serialized event
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public int RetryCount { get; private set; }        // Retry attempts
    public string? Error { get; private set; }         // Last error message
    public string? IdempotencyKey { get; private set; } // For duplicate prevention
    public DateTime? NextRetryAt { get; private set; } // Exponential backoff
    
    public static OutboxMessage Create(string eventType, string payload, string? idempotencyKey = null);
    public void MarkAsProcessed();
    public void RecordFailure(string errorMessage); // Sets NextRetryAt with exponential backoff
}
```

### 4.7 FeatureFlag Entity

The FeatureFlag entity enables dynamic feature toggles:

```csharp
public class FeatureFlag : IAggregateRoot
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }           // Unique feature name
    public bool IsEnabled { get; private set; }        // Toggle state
    public int? OrganizationId { get; private set; }  // Null = global flag
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    public static FeatureFlag Create(string name, int? organizationId = null, 
        string? description = null, bool isEnabled = false);
    public void Enable();
    public void Disable();
    public void UpdateDescription(string? description);
    
    public bool IsGlobal => OrganizationId == null;
    public bool IsOrganizationSpecific => OrganizationId != null;
}
```

### 4.8 OrganizationInvitation Entity

The OrganizationInvitation entity manages organization-level user invitations:

```csharp
public class OrganizationInvitation
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }          // Email address of invitee
    public GlobalRole Role { get; private set; }       // Admin or User
    public int OrganizationId { get; private set; }    // Multi-tenancy
    public int InvitedById { get; private set; }       // Admin who sent invite
    public string Token { get; private set; }          // Unique, secure random token
    public DateTime ExpiresAt { get; private set; }    // 72 hours from creation
    public DateTime CreatedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }  // Nullable
    public bool IsUsed { get; private set; }           // Default false
    
    public static OrganizationInvitation Create(string email, GlobalRole role, 
        int organizationId, int invitedById);
    public void MarkAsAccepted();
    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
    public bool CanBeAccepted() => !IsUsed && !IsExpired();
}
```

**Key Features:**
- Secure token generation (32 bytes, cryptographically secure, Base64Url encoded)
- 72-hour expiration period
- Idempotency via `IsUsed` flag
- Organization-level invitations (separate from project-level `Invitation` entity)

---

## 5. Application Layer

### 5.1 CQRS Implementation

The application layer uses CQRS (Command Query Responsibility Segregation) pattern:

- **Commands**: Write operations (Create, Update, Delete) - **Total: 98 Commands**
- **Queries**: Read operations (Get, List, Search) - **Total: 76 Queries**

### 5.2 Command Pattern

#### 5.2.1 Command Structure

```csharp
// Command (Request)
public record CreateProjectCommand(
    string Name,
    string Description,
    string Type,
    int SprintDurationDays,
    int OwnerId,
    string Status = "Active"
) : IRequest<CreateProjectResponse>;

// Response
public record CreateProjectResponse(int Id, string Name, string Description, string Type);
```

#### 5.2.2 Command Handler

```csharp
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, CreateProjectResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<CreateProjectResponse> Handle(
        CreateProjectCommand request, 
        CancellationToken cancellationToken)
    {
        // 1. Permission check
        // 2. Validation
        // 3. Business logic
        // 4. Persistence
        // 5. Return response
    }
}
```

#### 5.2.3 DeleteProjectCommandHandler

The `DeleteProjectCommandHandler` implements comprehensive permanent deletion of projects and all related entities:

**Deletion Order:**
1. ProjectTasks (direct project tasks)
2. SprintItems, KPISnapshots, Sprints (sprint-related entities)
3. ProjectMembers, Risks, Defects, Insights, Alerts, Activities (project-related entities)
4. DocumentStores, ProjectTeams, Notifications (relationship and metadata entities)
5. AIDecisions, AIAgentRuns (AI-related entities)
6. Tasks (Domain.Entities.Task linked to UserStories)
7. BacklogItems (Epics, Features, UserStories)
8. Comments and Attachments (polymorphic entities for all related entities)
9. Project itself

**Features:**
- Null-safety checks for navigation properties (e.g., `t.UserStory != null`)
- Proper cascade deletion order to respect foreign key constraints
- Cache invalidation for all affected users
- Comprehensive logging for audit purposes
- Transaction-based deletion (all or nothing)

### 5.3 Query Pattern

#### 5.3.1 Query Structure

```csharp
// Query (Request)
public record GetProjectByIdQuery(int ProjectId) : IRequest<GetProjectByIdResponse>;

// Response
public record GetProjectByIdResponse(
    int Id,
    string Name,
    string Description,
    string Type,
    string Status,
    List<ProjectMemberDto> Members,
    DateTimeOffset CreatedAt
);
```

#### 5.3.2 Query Handler

```csharp
public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, GetProjectByIdResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<GetProjectByIdResponse> Handle(
        GetProjectByIdQuery request, 
        CancellationToken cancellationToken)
    {
        // 1. Permission check (if needed)
        // 2. Query data
        // 3. Map to DTO
        // 4. Return response
    }
}
```

### 5.4 MediatR Behaviors

#### 5.4.1 Validation Behavior

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    // Automatically validates requests using FluentValidation
    // Throws ValidationException if validation fails
}
```

#### 5.4.2 Logging Behavior

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    // Logs all requests and responses
    // Measures execution time
}
```

### 5.5 Permission Classes

#### 5.5.1 GlobalPermissions

```csharp
public static class GlobalPermissions
{
    public static bool CanManageUsers(GlobalRole role) => role == GlobalRole.Admin;
    public static bool CanManageGlobalSettings(GlobalRole role) => role == GlobalRole.Admin;
    public static bool CanViewAllProjects(GlobalRole role) => role == GlobalRole.Admin;
    // ... other methods
}
```

#### 5.5.2 ProjectPermissions

```csharp
public static class ProjectPermissions
{
    public static bool CanEditProject(ProjectRole role) => 
        role is ProjectRole.ProductOwner or ProjectRole.ScrumMaster;
    
    public static bool CanDeleteProject(ProjectRole role) => 
        role == ProjectRole.ProductOwner;
    
    public static bool CanInviteMembers(ProjectRole role) => 
        role is ProjectRole.ProductOwner or ProjectRole.ScrumMaster;
    
    // ... other methods
}
```

### 5.6 Common Interfaces

#### 5.6.1 IUnitOfWork

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

#### 5.6.2 IRepository

```csharp
public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    IQueryable<TEntity> Query();
}
```

#### 5.6.3 IPermissionService

```csharp
public interface IPermissionService
{
    Task<List<string>> GetUserPermissionsAsync(int userId, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken ct = default);
}
```

#### 5.6.4 IDomainEventDispatcher

```csharp
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default);
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default);
}
```

#### 5.6.5 IFeatureFlagService

```csharp
public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(string featureName, int? organizationId = null, CancellationToken ct = default);
    Task<FeatureFlag?> GetAsync(string featureName, int? organizationId = null, CancellationToken ct = default);
    void InvalidateCache(string featureName, int? organizationId = null);
}
```

#### 5.6.6 IEmailService

```csharp
public interface IEmailService
{
    Task SendInvitationEmailAsync(...);              // Project-level invitations
    Task SendPasswordResetEmailAsync(...);
    Task SendWelcomeEmailAsync(...);
    Task<bool> SendOrganizationInvitationEmailAsync( // Organization-level invitations
        string recipientEmail,
        string recipientFirstName,
        string recipientLastName,
        string invitationLink,
        string organizationName,
        string inviterName,
        string role,
        CancellationToken ct = default);
}
```

#### 5.6.7 IPasswordHasher

```csharp
public interface IPasswordHasher
{
    (string Hash, string Salt) HashPassword(string password);
}
```

Interface for password hashing operations. Used by authentication services for secure password storage.

#### 5.6.8 IMentionParser

```csharp
public interface IMentionParser
{
    List<MentionDto> ParseMentions(string content);
    Task<List<int>> ResolveMentionedUserIds(List<MentionDto> mentions, int organizationId, CancellationToken ct);
}
```

Service for parsing `@username` mentions from comment content and resolving them to user IDs.

#### 5.6.9 INotificationPreferenceService

```csharp
public interface INotificationPreferenceService
{
    Task<bool> ShouldSendNotification(int userId, string notificationType, string channel, CancellationToken ct);
    Task InitializeDefaultPreferencesAsync(int userId, int organizationId, CancellationToken ct);
}
```

Service for checking user notification preferences and initializing default preferences for new users.

#### 5.6.10 IFileStorageService

```csharp
public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken ct);
    Task<Stream> GetFileAsync(string storedFileName, CancellationToken ct);
    Task DeleteFileAsync(string storedFileName, CancellationToken ct);
    string GenerateUniqueFileName(string originalFileName);
}
```

Service for file storage operations. Supports local file system or cloud storage implementations.

#### 5.6.11 IAIAvailabilityService

```csharp
public interface IAIAvailabilityService
{
    Task<bool> IsAIEnabledForOrganization(int organizationId, CancellationToken ct);
    Task ThrowIfAIDisabled(int organizationId, CancellationToken ct);
}
```

Service for checking if AI features are enabled for an organization. Provides caching for performance and throws exceptions when AI is disabled.

### 5.7 Projects CQRS

#### 5.7.1 Commands

```csharp
// Create Project
public record CreateProjectCommand(
    string Name,
    string Description,
    string Type,
    int SprintDurationDays,
    int OwnerId,
    string Status = "Active",
    DateTimeOffset? StartDate = null,
    List<int>? MemberIds = null
) : IRequest<CreateProjectResponse>;

// Update Project
public record UpdateProjectCommand(
    int ProjectId,
    int CurrentUserId,
    string? Name = null,
    string? Description = null,
    string? Status = null,
    string? Type = null,
    int? SprintDurationDays = null
) : IRequest<UpdateProjectResponse>;

// Archive Project (soft delete)
public record ArchiveProjectCommand(
    int ProjectId,
    int CurrentUserId
) : IRequest;

// Delete Project (permanent)
public record DeleteProjectCommand(
    int ProjectId,
    int CurrentUserId
) : IRequest;

// Assign Team to Project
public record AssignTeamToProjectCommand(
    int ProjectId,
    int TeamId,
    ProjectRole DefaultRole,
    Dictionary<int, ProjectRole>? MemberRoleOverrides = null
) : IRequest<AssignTeamToProjectResponse>;

// Add Member to Project
public record AddMemberToProjectCommand(
    int ProjectId,
    int UserId,
    ProjectRole Role,
    int InvitedById
) : IRequest<AddMemberToProjectResponse>;

// Remove Member from Project
public record RemoveMemberFromProjectCommand(
    int ProjectId,
    int UserId,
    int CurrentUserId
) : IRequest;

// Change Member Role
public record ChangeMemberRoleCommand(
    int ProjectId,
    int UserId,
    ProjectRole NewRole,
    int CurrentUserId
) : IRequest;
```

#### 5.7.2 Queries

```csharp
// Get User Projects (paginated)
public record GetUserProjectsQuery(
    int UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResponse<ProjectListDto>>;

// Get Project by ID
public record GetProjectByIdQuery(
    int ProjectId
) : IRequest<GetProjectByIdResponse>;

// Get Project Members
public record GetProjectMembersQuery(
    int ProjectId,
    int CurrentUserId
) : IRequest<List<ProjectMemberDto>>;

// Get User Role in Project
public record GetUserRoleInProjectQuery(
    int ProjectId,
    int UserId
) : IRequest<ProjectRole?>;
```

**Features:**
- `GetUserProjectsQuery`: Cached for 5 minutes, includes project members
- `GetUserRoleInProjectQuery`: Returns `ProjectRole` or `null` if user is not a member
- `DeleteProjectCommand`: Comprehensive deletion of all related entities in correct order
- `AssignTeamToProjectCommand`: Automatically adds all team members as project members

### 5.8 Organization Invitation CQRS

#### 5.8.1 Commands

```csharp
// Invite Organization User
public record InviteOrganizationUserCommand(
    string Email,
    GlobalRole Role,
    string FirstName,
    string LastName
) : IRequest<InviteOrganizationUserResponse>;

public record InviteOrganizationUserResponse(
    Guid InvitationId,
    string Email,
    string InvitationLink
);

// Accept Organization Invite
public record AcceptOrganizationInviteCommand(
    string Token,
    string Username,
    string Password,
    string ConfirmPassword
) : IRequest<AcceptInviteResponse>;
```

### 5.8 Comments CQRS

#### 5.8.1 Commands

```csharp
// Add Comment
public record AddCommentCommand(
    string EntityType,
    int EntityId,
    string Content,
    int? ParentCommentId = null
) : IRequest<AddCommentResponse>;

public record AddCommentResponse(
    int CommentId,
    int AuthorId,
    string AuthorName,
    string Content,
    DateTimeOffset CreatedAt,
    List<int> MentionedUserIds
);
```

**Features:**
- Validates entity exists before creating comment
- Parses `@username` mentions from content
- Creates `Mention` entities for tracking
- Publishes `CommentAddedEvent` and `UserMentionedEvent` via Outbox
- Supports comment threading with `ParentCommentId`

### 5.9 Settings CQRS

#### 5.8.1 GlobalSettings Entity

```csharp
public class GlobalSetting : IAggregateRoot
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;        // Unique setting key
    public string Value { get; set; } = string.Empty;       // Setting value
    public string? Description { get; set; }               // Optional description
    public string Category { get; set; } = "General";      // General, Security, Email, FeatureFlags
    public int? UpdatedById { get; set; }                   // User who last updated
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Navigation
    public User? UpdatedBy { get; set; }
}
```

**Setting Categories:**
- **General**: Application name, default timezone, default language, date format, project creation permissions
- **Security**: Token expiration, password policy (min length, uppercase, lowercase, digits, special chars), max login attempts, session duration, 2FA requirement
- **Email**: SMTP host, port, username, password, SSL/TLS, from email, from name
- **FeatureFlags**: Feature flag management (separate from FeatureFlag entity)

#### 5.8.2 Commands

```csharp
// Get Settings
public record GetSettingsQuery(string? Category = null) : IRequest<Dictionary<string, string>>;

// Update Setting
public record UpdateSettingCommand(
    string Key,
    string Value,
    string? Category = null
) : IRequest<UpdateSettingResponse>;

public record UpdateSettingResponse(
    string Key,
    string Value,
    string Category
);

// Send Test Email
public record SendTestEmailCommand(string Email) : IRequest<SendTestEmailResponse>;

public record SendTestEmailResponse(bool Success, string Message);
```

### 5.10 Feature Flag CQRS

#### 5.9.1 Commands

```csharp
// Create Feature Flag
public record CreateFeatureFlagCommand(
    string Name,
    string? Description,
    bool IsEnabled,
    int? OrganizationId
) : IRequest<FeatureFlagDto>;

// Update Feature Flag
public record UpdateFeatureFlagCommand(
    Guid Id,
    bool? IsEnabled,
    string? Description
) : IRequest<FeatureFlagDto>;
```

#### 5.10.2 Queries

```csharp
public record GetAllFeatureFlagsQuery(int? OrganizationId = null) : IRequest<List<FeatureFlagDto>>;

public record GetFeatureFlagByNameQuery(string Name, int? OrganizationId = null) : IRequest<FeatureFlagDto?>;

public record FeatureFlagDto(
    Guid Id,
    string Name,
    bool IsEnabled,
    int? OrganizationId,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsGlobal,
    bool IsOrganizationSpecific
);
```

---

## 6. Infrastructure Layer

### 6.1 Database Contexts

#### 6.1.1 AppDbContext (SQL Server)

Primary database context for transactional data:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
    // ... other DbSets
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entity configurations
        // Relationships
        // Indexes
    }
}
```

**Features:**
- Multi-tenancy support via `OrganizationId` filtering
- Soft delete support (where applicable)
- Optimistic concurrency control (`RowVersion`)
- Cascade delete restrictions (prevents cycles)

#### 6.1.2 VectorDbContext (PostgreSQL)

Vector database context for AI agent memory:

```csharp
public class VectorDbContext : DbContext
{
    public DbSet<AgentMemoryRecord> AgentMemories { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // pgvector configurations
    }
}
```

### 6.2 Repository Pattern

#### 6.2.1 GenericRepository

```csharp
public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<TEntity> _dbSet;
    
    // Implements IRepository<TEntity> interface
    // Provides CRUD operations
    // Supports querying via Query() method
}
```

#### 6.2.2 UnitOfWork

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    
    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        // Returns cached or creates new repository instance
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Saves all changes in a single transaction
    }
}
```

### 6.3 Identity Services

#### 6.3.1 AuthService

```csharp
public class AuthService : IAuthService
{
    public async Task<(string AccessToken, string RefreshToken)> LoginAsync(
        string username, 
        string password, 
        CancellationToken ct);
    
    public async Task<int> RegisterAsync(
        string username, 
        string email, 
        string password, 
        string firstName, 
        string lastName, 
        CancellationToken ct);
}
```

**Login Behavior:**
- Updates `User.LastLoginAt` to `DateTimeOffset.UtcNow` on successful login
- Updates `User.UpdatedAt` timestamp
- Supports login with either username or email
- Validates user is active before allowing login

#### 6.3.2 JwtTokenService

```csharp
public class JwtTokenService : ITokenService
{
    public string GenerateAccessToken(int userId, string username, string email, List<string> roles);
    public string GenerateRefreshToken();
    public ClaimsPrincipal? ValidateToken(string token);
}
```

#### 6.3.3 PasswordHasher

```csharp
public class PasswordHasher
{
    public string HashPassword(string password, out string salt);
    public bool VerifyPassword(string password, string hash, string salt);
}
```

### 6.4 Permission Service

```csharp
public class PermissionService : IPermissionService
{
    // Queries RolePermission entities based on user's GlobalRole
    // Returns list of permission names (e.g., "projects.create")
    // Implements caching (5-minute expiration)
    
    public async Task<List<string>> GetUserPermissionsAsync(int userId, CancellationToken ct);
    public async Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken ct);
}
```

### 6.5 Email Service

#### 6.5.1 EmailService (Stub)

Default email service that logs emails (for development).

#### 6.5.2 SmtpEmailService

Production email service using SMTP:

```csharp
public class SmtpEmailService : IEmailService
{
    public async Task SendInvitationEmailAsync(...);
    public async Task SendPasswordResetEmailAsync(...);
    public async Task SendWelcomeEmailAsync(...);
}
```

**Templates:**
- `MemberInvitation.html` - Project-level invitations
- `OrganizationInvitation.html` - Organization-level invitations (French)
- `PasswordReset.html`
- `Welcome.html`

#### 6.5.3 SendOrganizationInvitationEmailAsync

```csharp
Task<bool> SendOrganizationInvitationEmailAsync(
    string recipientEmail,
    string recipientFirstName,
    string recipientLastName,
    string invitationLink,
    string organizationName,
    string inviterName,
    string role,
    CancellationToken ct = default);
```

**Features:**
- Loads `OrganizationInvitation.html` template
- Replaces placeholders: `{{FirstName}}`, `{{LastName}}`, `{{Email}}`, `{{InvitationLink}}`, `{{OrganizationName}}`, `{{InviterName}}`, `{{Role}}`
- Subject: `"Invitation à rejoindre {organizationName} sur IntelliPM"`
- Returns `true` on success, `false` on failure

#### 6.5.4 SendTestEmailAsync

```csharp
Task SendTestEmailAsync(string email, CancellationToken ct = default);
```

**Features:**
- Sends a test email to verify SMTP configuration
- Uses a predefined HTML template with IntelliPM branding
- Subject: `"IntelliPM - Test Email"`
- Useful for testing email settings in the admin panel

### 6.6 Cache Service

```csharp
public class CacheService : ICacheService
{
    // In-memory caching
    // Prefix-based cache invalidation
    // TTL support
    
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
}
```

### 6.7 Comment and Mention Services

#### 6.7.1 MentionParser

```csharp
public class MentionParser : IMentionParser
{
    private readonly IUnitOfWork _unitOfWork;
    private static readonly Regex MentionRegex = new Regex(@"@([a-zA-Z0-9._-]+)", RegexOptions.Compiled);
    
    public List<MentionDto> ParseMentions(string content);
    public async Task<List<int>> ResolveMentionedUserIds(List<MentionDto> mentions, int organizationId, CancellationToken ct);
}
```

**Features:**
- Uses regex to parse `@username` mentions from text
- Stores mention position (StartIndex, Length) for highlighting
- Resolves usernames to user IDs within same organization
- Supports usernames with alphanumeric, dots, underscores, hyphens

#### 6.7.2 NotificationPreferenceService

```csharp
public class NotificationPreferenceService : INotificationPreferenceService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<bool> ShouldSendNotification(int userId, string notificationType, string channel, CancellationToken ct);
    public async Task InitializeDefaultPreferencesAsync(int userId, int organizationId, CancellationToken ct);
}
```

**Features:**
- Checks user preferences before sending notifications
- Falls back to default preferences if user hasn't configured
- Supports multiple channels (email, in-app, push)
- Supports frequency settings (instant, daily, weekly, never)
- Initializes default preferences for new users

### 6.8 File Storage Service

#### 6.8.1 LocalFileStorageService

```csharp
public class LocalFileStorageService : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _uploadDirectory;
    
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken ct);
    public Task<Stream> GetFileAsync(string storedFileName, CancellationToken ct);
    public Task DeleteFileAsync(string storedFileName, CancellationToken ct);
    public string GenerateUniqueFileName(string originalFileName);
}
```

**Features:**
- Stores files locally in configured directory (default: "uploads")
- Generates unique filenames with timestamp + GUID
- Tracks original filename for display to users
- Supports file metadata (name, size, type, extension)
- Implements soft delete (don't delete files immediately)

**Configuration:**
```json
{
  "FileStorage": {
    "UploadDirectory": "uploads",
    "MaxFileSizeMB": 10,
    "MaxTotalSizeMB": 50
  }
}
```

### 6.9 Organization Permission Policy Service

#### 6.9.1 OrganizationPermissionPolicyService

```csharp
public class OrganizationPermissionPolicyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrganizationPermissionPolicyService> _logger;
    
    // Gets the organization permission policy
    public async Task<OrganizationPermissionPolicy?> GetPolicyAsync(int organizationId, CancellationToken ct);
    
    // Checks if a permission is allowed for the organization
    public async Task<bool> IsPermissionAllowedAsync(int organizationId, string permission, CancellationToken ct);
    
    // Validates that all given permissions are allowed
    public async Task ValidatePermissionsAsync(int organizationId, IEnumerable<string> permissions, CancellationToken ct);
    
    // Gets all allowed permissions for an organization
    public async Task<List<string>> GetAllowedPermissionsAsync(int organizationId, CancellationToken ct);
}
```

**Features:**
- **Policy Retrieval**: Gets organization permission policy (returns null if not exists)
- **Permission Checking**: Checks if a specific permission is allowed
- **Bulk Validation**: Validates multiple permissions at once (throws exception if any disallowed)
- **Default Behavior**: Returns empty list if no policy exists (indicating all permissions allowed)
- **Used By**: `UpdateMemberPermissionCommandHandler`, `UpdateRolePermissionsCommandHandler`

**Default Behavior:**
- No policy exists → all permissions allowed
- Policy inactive (`IsActive = false`) → all permissions allowed
- Policy active with empty permissions → all permissions allowed
- Policy active with permissions → only listed permissions allowed

### 6.10 AI Availability Service

#### 6.10.1 AIAvailabilityService

```csharp
public class AIAvailabilityService : IAIAvailabilityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private const int CacheExpirationMinutes = 5;
    
    public async Task<bool> IsAIEnabledForOrganization(int organizationId, CancellationToken ct);
    public async Task ThrowIfAIDisabled(int organizationId, CancellationToken ct);
}
```

**Features:**
- Checks if organization has disabled AI quota
- Uses caching (5-minute expiration) for performance
- Throws `UnauthorizedException` if AI is disabled
- Used by AI agents to check availability before execution

### 6.11 AI Services

#### 6.7.1 Semantic Kernel Agent Service

```csharp
public class SemanticKernelAgentService : IAgentService
{
    // Uses Microsoft Semantic Kernel
    // Integrates with Ollama LLM
    // Supports function calling via plugins
}
```

#### 6.7.2 Agent Plugins

- **ProjectInsightPlugin**: Analyzes project health
- **RiskDetectionPlugin**: Detects project risks
- **SprintPlanningPlugin**: Assists with sprint planning
  - Functions: GetBacklogTasks, GetTeamCapacity, GetSprintCapacity
  - Used for sprint planning suggestions
- **SprintRetrospectivePlugin**: Generates sprint retrospectives
  - Functions: GetSprintMetrics, GetCompletedTasks, GetIncompleteTasks, GetSprintDefects, GetTeamActivity
  - Used for retrospective generation after sprint completion
  - Validates sprint status (must be Completed)
  - Saves retrospective notes to Sprint.RetrospectiveNotes
- **TaskDependencyPlugin**: Analyzes task dependencies
- **TaskQualityPlugin**: Evaluates task quality (for task improvement)

#### 6.7.3 Agent Handlers

Located in `Infrastructure/AI/Handlers/`:
- `AnalyzeProjectHandler`
- `DetectRisksHandler`
- `PlanSprintHandler`

**Note:** These handlers should be moved to Application layer for Clean Architecture compliance.

### 6.12 Vector Store

#### 6.12.1 PostgresVectorStore

```csharp
public class PostgresVectorStore : IVectorStore
{
    // Stores and retrieves vector embeddings
    // Uses pgvector for similarity search
    // Used by AI agents for memory
}
```

### 6.13 Health Checks

- **DatabaseHealthCheck**: Verifies SQL Server connectivity
- **OllamaHealthCheck**: Verifies Ollama LLM availability
- **MemoryHealthCheck**: Monitors memory usage

### 6.14 Domain Event Dispatcher

```csharp
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventDispatcher> _logger;
    
    // Dispatches domain events via MediatR
    // Logs each dispatched event
    // Handles exceptions gracefully
    
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct);
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct);
}
```

### 6.15 Outbox Processor (Background Service)

The `OutboxProcessor` is a background service that implements the Outbox pattern:

```csharp
public class OutboxProcessor : BackgroundService
{
    // Polling interval: 5 seconds
    // Max retry attempts: 3
    // Exponential backoff: 2^RetryCount minutes
    
    // Features:
    // - Fetches unprocessed messages
    // - Checks idempotency before processing
    // - Deserializes and dispatches domain events
    // - Marks messages as processed on success
    // - Records failures with NextRetryAt for retry scheduling
    // - Uses IServiceScopeFactory for scoped DbContext
}
```

**Key Features:**
- **Idempotency**: Checks `IdempotencyKey` to prevent duplicate event processing
- **Exponential Backoff**: Failed messages are retried at 2, 4, 8 minute intervals
- **Max Retries**: After 3 attempts, messages are marked as permanently failed
- **Scoped Processing**: Uses `IServiceScopeFactory` for proper DbContext lifecycle

### 6.16 Feature Flag Service

```csharp
public class FeatureFlagService : IFeatureFlagService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    
    // Checks if a feature is enabled for an organization or globally
    // Prioritizes organization-specific flags over global flags
    // Caches results for 5 minutes
    
    public async Task<bool> IsEnabledAsync(string featureName, int? organizationId = null, CancellationToken ct);
    public async Task<FeatureFlag?> GetAsync(string featureName, int? organizationId = null, CancellationToken ct);
    public void InvalidateCache(string featureName, int? organizationId = null);
}
```

**Lookup Priority:**
1. Organization-specific flag (if `organizationId` provided)
2. Global flag (if no organization-specific flag found)

---

## 7. API Layer

### 7.1 Controllers

All controllers inherit from `BaseApiController`:

```csharp
[ApiController]
[Authorize]
public abstract class BaseApiController : ControllerBase
{
    protected int GetCurrentUserId();
    protected string? GetCurrentUsername();
    protected string? GetCurrentUserEmail();
    protected bool HasRole(string role);
    protected int GetOrganizationId();
}
```

#### 7.1.1 Controllers List

| Controller | Purpose | Endpoints |
|------------|---------|-----------|
| `ProjectsController` | Project management | CRUD operations, members, invites, assign team |
| `TasksController` | Task management | CRUD operations, assignments, status |
| `SprintsController` | Sprint management | CRUD operations, start/complete |
| `TeamsController` | Team management | CRUD operations, capacity |
| `DefectsController` | Defect tracking | CRUD operations |
| `MilestonesController` | Milestone management | CRUD operations, complete, statistics |
| `ReleasesController` | Release management | CRUD operations, deploy, sprint management, release notes, quality gates (17 endpoints) |
| `UsersController` | User management | CRUD operations, list, get user projects, get user activity |
| `Admin/UsersController` | Admin user management | Invite organization users |
| `AuthController` | Authentication | Login, register (deprecated), refresh token, invite acceptance |
| `NotificationsController` | Notifications | Get, mark read |
| `MetricsController` | Metrics & analytics | Velocity, burndown, distribution |
| `AgentsController` | AI agents | Run agents (product, delivery, manager, QA, business), store notes (requires `projects.view` permission) |
| `AgentController` | AI agent operations | Improve task, analyze project, detect risks, plan sprint, generate retrospective, metrics, audit logs (7 endpoints) |
| `SearchController` | Search | Global search |
| `SettingsController` | Settings | Get/update global settings, send test email |
| `PermissionsController` | Permissions | Get matrix, update permissions |
| `BacklogController` | Backlog | Create backlog items |
| `AlertsController` | Alerts | Get alerts |
| `InsightsController` | Insights | Get project insights |
| `ActivityController` | Activity feed | Get recent activity |
| `HealthController` | Health checks | Health status (database, Ollama, memory) |
| `HealthApiController` | API smoke tests | Endpoint routing and authentication checks |
| `FeatureFlagsController` | Feature flags (Public read) | GET operations for all authenticated users |
| `Admin/FeatureFlagsController` | Feature flags (Admin) | CRUD operations for admin management |
| `Admin/DashboardController` | Admin dashboard | System statistics and overview |
| `Admin/AuditLogsController` | Audit logs | View system audit logs |
| `Admin/SystemHealthController` | System health | Detailed health check information |
| `Admin/DeadLetterQueueController` | Dead Letter Queue | View, retry, and delete DLQ messages |
| `Admin/ReadModelsController` | Read Models (Admin) | Rebuild read models, get read model data |
| `Admin/AIGovernanceController` | AI Governance (Admin) | Manage AI quotas, disable/enable AI, view all decisions |
| `Admin/AdminMemberPermissionsController` | Member Permissions (Admin) | View and update member roles/permissions within own organization |
| `Admin/AdminAIQuotaController` | AI Quota Management (Admin) | Manage AI quotas for organization members |
| `Admin/OrganizationsController` | Organizations (Admin) | Manage organizations (SuperAdmin access) |
| `Admin/OrganizationController` | Organization (Admin) | Manage own organization details |
| `SuperAdmin/SuperAdminAIQuotaController` | SuperAdmin AI Quota | Manage organization AI quotas (SuperAdmin only, uses versioned routes) |
| `SuperAdmin/SuperAdminPermissionPolicyController` | Permission Policies (SuperAdmin) | Manage organization permission policies (uses versioned routes) |
| `AIGovernanceController` | AI Governance (User) | View AI decisions, quota status, usage statistics |
| `ReadModelsController` | Read Models (User) | Get read model data for current organization |
| `TestController` | Testing | Test endpoints (DEBUG-only, #if DEBUG) |
| ~~`AdminHashGeneratorController`~~ | ~~Admin utilities~~ | ~~REMOVED - Security vulnerability~~ |

### 7.2 Middleware

#### 7.2.1 SentryUserContextMiddleware

Captures user context for Sentry error tracking:

```csharp
public class SentryUserContextMiddleware
{
    // Adds user ID, username, email to Sentry context
    // Executes after authentication middleware
}
```

#### 7.2.2 Security Headers Middleware

Adds security headers to all responses:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Content-Security-Policy`
- `Referrer-Policy`
- `Permissions-Policy`

#### 7.2.3 Cookie Authentication Middleware

Reads JWT token from HTTP-only cookie and adds it to Authorization header:

```csharp
// Middleware in Program.cs
app.Use(async (context, next) =>
{
    // Only add Authorization header from cookie if it's not already present
    if (string.IsNullOrEmpty(context.Request.Headers.Authorization))
    {
        // Check if token is in cookie
        if (context.Request.Cookies.TryGetValue("auth_token", out var token) && !string.IsNullOrEmpty(token))
        {
            // Add to Authorization header for JWT middleware
            context.Request.Headers.Authorization = $"Bearer {token}";
        }
        else if (context.Request.Path.StartsWithSegments("/api/v1/Auth/me"))
        {
            // Log for debugging - only for /api/Auth/me endpoint
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            var hasCookie = context.Request.Cookies.ContainsKey("auth_token");
            var cookieValue = hasCookie ? "present but empty" : "missing";
            var cookieNames = string.Join(", ", context.Request.Cookies.Keys);
            logger.LogWarning(
                "Auth/me request - Cookie: {CookieStatus}, All cookies: {CookieCount}, Cookie names: {CookieNames}, Origin: {Origin}, Referer: {Referer}",
                cookieValue, context.Request.Cookies.Count, cookieNames,
                context.Request.Headers.Origin.ToString(),
                context.Request.Headers.Referer.ToString());
        }
    }
    await next();
});
```

**Features:**
- Automatically reads `auth_token` cookie and adds to `Authorization` header
- Logs detailed cookie information for `/api/v1/Auth/me` endpoint debugging
- Only adds header if not already present (allows manual Authorization header)
- Executes before JWT authentication middleware

#### 7.2.4 FeatureFlagMiddleware

Checks feature flags in the request pipeline:

```csharp
public class FeatureFlagMiddleware
{
    // Extracts feature flag name from "X-Feature-Flag" header
    // Queries IFeatureFlagService to check if feature is enabled
    // Gets OrganizationId from current user context
    // Returns 403 Forbidden if feature is disabled
    // Continues pipeline if feature is enabled
    // Fails open: allows request on service errors
}
```

**Usage:**
```http
GET /api/v1/projects HTTP/1.1
X-Feature-Flag: EnableAdvancedMetrics
```

### 7.3 Authorization

#### 7.3.1 RequirePermissionAttribute

```csharp
[RequirePermission("projects.create")]
public async Task<IActionResult> CreateProject(...)
{
    // Handler requires "projects.create" permission
}
```

**Note:** Authorization policy handler needs to be implemented in `Program.cs`.

### 7.4 Rate Limiting

Configured in `Program.cs`:

- **Global Limit**: 100 requests/minute per user/IP
- **Auth Limit**: 30 requests/minute per IP (increased from 5 to prevent 429 errors)
- **AI Limit**: 10 requests/minute per user

**Rate Limiter Configuration:**
- Uses `PartitionedRateLimiter` with user ID or IP address as partition key
- Fixed window rate limiting with automatic replenishment
- Queue limit: 0 (rejects immediately when limit exceeded)
- Applied via `[EnableRateLimiting("auth")]` or `[EnableRateLimiting("ai")]` attributes

### 7.5 API Versioning

- **Default Version**: v1.0
- **Version Readers**: URL segment, Header (`X-Api-Version`), Media type
- **Format**: `api/v{version}/[controller]` for standard controllers
- **Admin Routes**: Admin controllers use `/api/admin/...` without versioning in URL (e.g., `/api/admin/users`, `/api/admin/feature-flags`)
- **SuperAdmin Routes**: SuperAdmin controllers use `/api/v1/superadmin/...` with versioning (e.g., `/api/v1/superadmin/organizations/{orgId}/ai-quota`)
- **Special Routes**: Some controllers use explicit routes (e.g., `/api/v1/feature-flags` uses explicit route for kebab-case compatibility)

---

## 8. Security

### 8.1 Authentication

#### 8.1.1 JWT Authentication

- **Algorithm**: HS256 (HMAC-SHA256)
- **Access Token**: 15 minutes expiration
- **Refresh Token**: 7 days expiration
- **Claims**: UserId, Username, Email, Roles, OrganizationId

#### 8.1.2 Cookie-Based Authentication

The backend uses HTTP-only cookies for secure token storage:

- **Access Token Cookie**: `auth_token` (httpOnly, Secure in production, SameSite=Strict, 15 minutes expiration)
- **Refresh Token Cookie**: `refresh_token` (httpOnly, Secure in production, SameSite=Strict, 7 days expiration)
- **Cookie Middleware**: Automatically reads `auth_token` cookie and adds it to `Authorization` header for JWT middleware
- **CORS Configuration**: Cookies are sent with `credentials: 'include'` from frontend
- **Token Refresh**: Frontend automatically attempts token refresh on 401 errors before redirecting to login

**Cookie Configuration:**
- Development: `Secure = false` (allows HTTP)
- Production: `Secure = true` (HTTPS only)
- Domain: `null` (browser handles domain)
- Path: `/` (available for all routes)

**Middleware Flow:**
1. Request arrives with `auth_token` cookie
2. Custom middleware reads cookie and adds `Authorization: Bearer {token}` header
3. JWT middleware validates token and sets `User` claims
4. Controller can access user via `GetCurrentUserId()` from `BaseApiController`

#### 8.1.3 Password Security

- **Hashing**: Custom implementation with salt
- **Salt**: Unique per user
- **Storage**: Separate `PasswordHash` and `PasswordSalt` fields

### 8.2 Authorization

#### 8.2.1 Global Roles

- **Admin**: Full system access
- **User**: Standard user access

#### 8.2.2 Project Roles

- **ProductOwner**: Full project control
- **ScrumMaster**: Sprint and team management
- **Developer**: Task management
- **Tester**: Testing and defect management
- **Viewer**: Read-only access

#### 8.2.3 Permission System

**Database-Driven Permissions:**
- `Permission` entity stores permission definitions
- `RolePermission` entity links `GlobalRole` to `Permission`
- `PermissionService` evaluates permissions

**Static Permission Classes:**
- `GlobalPermissions`: System-level permissions
- `ProjectPermissions`: Project-level permissions

### 8.3 Multi-Tenancy

#### 8.3.1 Organization Isolation

- All entities have `OrganizationId` (where applicable)
- Queries automatically filter by `OrganizationId`
- `CurrentUserService` provides organization context

#### 8.3.2 Current Implementation

**Entities with OrganizationId:**
- Project, User, ProjectTask, Team, Defect, Sprint, Notification, Invitation

**Entities Missing OrganizationId (should be added):**
- Activity, Alert, BacklogItem, DocumentStore, AIAgentRun, AIDecision, Insight, Risk

### 8.4 Security Headers

All responses include security headers (configured in middleware).

### 8.5 CORS

Configured for frontend origins:
- Development: `http://localhost:3000`, `http://localhost:3001`
- Production: Configurable via `AllowedOrigins` setting

---

## 9. Database

### 9.1 Database Architecture

#### 9.1.1 SQL Server (Primary)

**Purpose**: Transactional data, business entities

**Connection String**: `SqlServer` or `DefaultConnection`

**Features:**
- Entity Framework Core migrations
- Optimistic concurrency control (`RowVersion`)
- Soft deletes (where applicable)
- Multi-tenancy via `OrganizationId`

#### 9.1.2 PostgreSQL (Vector Store)

**Purpose**: AI agent memory, vector embeddings

**Connection String**: `VectorDb`

**Features:**
- pgvector extension for vector similarity search
- Used by AI agents for context and memory

### 9.2 Migrations

#### 9.2.1 Creating Migrations

```bash
# SQL Server migrations
dotnet ef migrations add MigrationName --project IntelliPM.Infrastructure --startup-project IntelliPM.API --context AppDbContext

# PostgreSQL migrations
dotnet ef migrations add MigrationName --project IntelliPM.Infrastructure --startup-project IntelliPM.API --context VectorDbContext
```

#### 9.2.2 Applying Migrations

Migrations are automatically applied on application startup (in `Program.cs`).

**Manual Application:**
```bash
dotnet ef database update --project IntelliPM.Infrastructure --startup-project IntelliPM.API --context AppDbContext
```

### 9.3 Database Schema

#### 9.3.1 Core Tables

- **Organizations**: Organization definitions
- **Users**: User accounts with authentication
- **Projects**: Project definitions
- **ProjectMembers**: User-project relationships with roles
- **ProjectTasks**: Direct project tasks
- **Sprints**: Sprint definitions
- **Defects**: Defect tracking
- **Teams**: Team definitions
- **Notifications**: User notifications
- **Activities**: Activity feed entries
- **Permissions**: Permission definitions
- **RolePermissions**: Role-permission mappings
- **GlobalSettings**: System-wide settings with categories (General, Security, Email, FeatureFlags)
- **OutboxMessages**: Outbox pattern for reliable event publishing
- **DeadLetterMessages**: Dead Letter Queue for failed outbox messages
- **FeatureFlags**: Feature toggle configurations
- **OrganizationInvitations**: Organization-level user invitations
- **AuditLogs**: System audit logs for tracking user actions
- **ProjectTeams**: Many-to-many relationship between Projects and Teams
- **Comments**: Comments on tasks, projects, sprints, defects (polymorphic)
- **Mentions**: User mentions (@username) in comments
- **NotificationPreferences**: User notification preferences
- **Attachments**: File attachments for entities
- **AIDecisionLogs**: AI decision logging for governance
- **AIQuotas**: AI quota and usage tracking per organization
- **TaskBoardReadModels**: CQRS read model for task board views
- **SprintSummaryReadModels**: CQRS read model for sprint summaries
- **ProjectOverviewReadModels**: CQRS read model for project overviews

#### 9.3.2 Relationships

- **Project → Organization**: Many-to-One
- **User → Organization**: Many-to-One
- **Project → ProjectMembers**: One-to-Many
- **Project → Sprints**: One-to-Many
- **Project → ProjectTasks**: One-to-Many
- **Sprint → ProjectTasks**: One-to-Many
- **User → ProjectTasks** (Assignee): One-to-Many

### 9.4 Indexes

Key indexes:
- `Organizations.Name`
- `Users.Email`
- `Users.Username`
- `Projects.OrganizationId`
- `ProjectTasks.ProjectId`
- `ProjectTasks.OrganizationId`
- `Sprints.ProjectId`
- `Sprints.OrganizationId`

**OutboxMessages Indexes:**
- `IX_OutboxMessages_ProcessedAt`: Filter unprocessed messages
- `IX_OutboxMessages_CreatedAt`: Order by creation time
- `IX_OutboxMessages_IdempotencyKey`: Idempotency checks (filtered)
- `IX_OutboxMessages_NextRetryAt`: Retry scheduling (filtered)
- `IX_OutboxMessages_ProcessedAt_CreatedAt`: Composite for common queries
- `IX_OutboxMessages_ProcessedAt_NextRetryAt_RetryCount`: Retry query optimization

**FeatureFlags Indexes:**
- `IX_FeatureFlags_Name`: Quick name lookups
- `IX_FeatureFlags_Name_OrganizationId`: **Unique composite** (prevents duplicates)
- `IX_FeatureFlags_OrganizationId`: Organization filtering (filtered)
- `IX_FeatureFlags_IsEnabled`: Filter by enabled status
- `IX_FeatureFlags_OrganizationId_IsEnabled`: Common query pattern

**GlobalSettings Indexes:**
- `IX_GlobalSettings_Key`: **Unique** (prevents duplicate setting keys)
- `IX_GlobalSettings_UpdatedById`: Foreign key index for UpdatedBy relationship

---

## 10. Configuration

### 10.1 Configuration Files

#### 10.1.1 appsettings.json

Base configuration file with default settings.

#### 10.1.2 appsettings.Development.json

Development-specific settings:
- Lower log levels
- Development database connections
- Local LLM endpoints

#### 10.1.3 appsettings.Production.json

Production-specific settings:
- Higher log levels
- Production database connections
- Production LLM endpoints

#### 10.1.4 appsettings.Testing.json

Testing-specific settings for integration tests.

### 10.2 Configuration Sections

#### 10.2.1 Connection Strings

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=...;Database=IntelliPM;...",
    "VectorDb": "Host=...;Database=intellipm_vectors;..."
  }
}
```

#### 10.2.2 JWT Settings

```json
{
  "Jwt": {
    "SecretKey": "...",  // Minimum 32 characters
    "Issuer": "IntelliPM",
    "Audience": "IntelliPM.API"
  }
}
```

#### 10.2.3 Ollama Settings

```json
{
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "Model": "llama3.2:3b"
  }
}
```

#### 10.2.4 Email Settings

```json
{
  "Email": {
    "Provider": "SMTP",
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "SmtpUsername": "...",
    "SmtpPassword": "...",
    "FromEmail": "noreply@intellipm.com",
    "FromName": "IntelliPM"
  }
}
```

#### 10.2.5 Sentry Settings

```json
{
  "Sentry": {
    "Dsn": "...",
    "Environment": "production",
    "TracesSampleRate": 0.1,      // 10% in production
    "ProfilesSampleRate": 0.1
  }
}
```

**Sentry Features:**
- Performance tracking with configurable sample rates
- Request/response logging (without PII)
- Stack trace attachment
- Environment-aware configuration
- TracesSampler for granular control

#### 10.2.6 JSON Serialization

Enums are serialized as strings instead of numbers for better API compatibility and readability.

**Configuration:**
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings instead of numbers (e.g., "Admin" instead of 2)
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
```

**Examples:**
- `GlobalRole.Admin` serializes as `"Admin"` instead of `2`
- `ProjectRole.ProductOwner` serializes as `"ProductOwner"` instead of `1`
- This applies to all enum types in API responses

**Benefits:**
- More readable API responses
- Better frontend integration (easier to parse and display)
- Reduces risk of enum value mismatches when enum definitions change

#### 10.2.7 Serilog Settings

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/intellipm-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithProcessId",
      "WithThreadId",
      "WithExceptionDetails"
    ]
  }
}
```

**Logging Sinks:**
- **Console**: Structured output with timestamps
- **File**: Rolling daily logs (7-day retention in dev, 30-day in production)
- **Seq**: Optional centralized logging (enabled via `SEQ_URL` environment variable)

**Enrichment:**
- Machine name, Process ID, Thread ID
- Environment name
- Exception details
- Request context (host, scheme, user agent, IP address)

#### 10.2.8 Rate Limiting

```json
{
  "RateLimiting": {
    "Global": {
      "PermitLimit": 100,
      "WindowMinutes": 1
    },
    "Auth": {
      "PermitLimit": 30,
      "WindowMinutes": 1
    },
    "AI": {
      "PermitLimit": 10,
      "WindowMinutes": 1
    }
  }
}
```

**Note:** Auth rate limit was increased from 5 to 30 requests/minute to prevent 429 errors during development and testing.

### 10.3 User Secrets

For sensitive configuration, use .NET User Secrets:

```bash
dotnet user-secrets init --project IntelliPM.API
dotnet user-secrets set "Jwt:SecretKey" "your-secret-key" --project IntelliPM.API
```

See `SETUP_USER_SECRETS.md` for detailed instructions.

---

## 11. Development Setup

### 11.1 Prerequisites

- **.NET 8.0 SDK**
- **SQL Server** (LocalDB or full instance)
- **PostgreSQL** (with pgvector extension)
- **Ollama** (for AI agents)
- **Visual Studio 2022** or **VS Code** with C# extension

### 11.2 Initial Setup

#### 11.2.1 Clone Repository

```bash
git clone <repository-url>
cd intelliPM\ V2\backend
```

#### 11.2.2 Restore Dependencies

```bash
dotnet restore
```

#### 11.2.3 Configure Databases

1. **SQL Server**: Create database `IntelliPM`
2. **PostgreSQL**: 
   - Create database `intellipm_vectors`
   - Install pgvector extension:
   ```sql
   CREATE EXTENSION IF NOT EXISTS vector;
   ```

#### 11.2.4 Configure App Settings

1. Copy `appsettings.Development.json` and update connection strings
2. Set up User Secrets for JWT secret key

#### 11.2.5 Run Migrations

Migrations run automatically on startup, or manually:

```bash
dotnet ef database update --project IntelliPM.Infrastructure --startup-project IntelliPM.API --context AppDbContext
```

#### 11.2.6 Start Ollama

```bash
ollama serve
ollama pull llama3.2:3b
```

#### 11.2.7 Run Application

```bash
dotnet run --project IntelliPM.API
```

Application runs on `https://localhost:5001` or `http://localhost:5000`.

### 11.3 Development Tools

#### 11.3.1 Swagger UI

Available at `/swagger` in development mode.

#### 11.3.2 Health Checks UI

Available at `/health-ui` for monitoring system health.

#### 11.3.3 Logs

Logs are written to:
- Console (structured logging)
- `logs/intellipm-YYYYMMDD.log` (file)

### 11.4 Code Style

- Follow C# coding conventions
- Use meaningful names
- Add XML documentation comments
- Keep methods focused and small

---

## 12. Testing

### 12.1 Test Projects

- **IntelliPM.Tests.API**: API integration tests
  - `ReleasesControllerTests.cs`: Comprehensive unit tests for ReleasesController (69 tests covering all 17 endpoints)
- **IntelliPM.Tests.Application**: Application layer unit tests
- **IntelliPM.Tests.Infrastructure**: Infrastructure layer tests
- **IntelliPM.Tests.E2E**: End-to-end tests

### 12.2 Running Tests

```bash
dotnet test
```

### 12.3 Test Coverage

Target: 80% code coverage

### 12.4 Test Infrastructure

#### 12.4.1 Custom WebApplicationFactory

Integration tests use a custom `WebApplicationFactory` with the following configurations:

**JWT Configuration:**
- Sets JWT environment variables (`Jwt__SecretKey`, `Jwt__Issuer`, `Jwt__Audience`) before configuration loads
- Ensures consistent authentication setup across all tests

**Content Root Path:**
- Configures `ContentRootPath` to use temporary directory for testing
- Prevents file system permission errors in test environments

**Example:**
```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment variables BEFORE any configuration is loaded
        Environment.SetEnvironmentVariable("Jwt__SecretKey", "YourSecureSecretKeyOfAt32CharactersMinimumForTesting!");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "TestIssuer");
        Environment.SetEnvironmentVariable("Jwt__Audience", "TestAudience");

        builder.UseContentRoot(Path.GetTempPath()); // Set content root to temporary path
        // ... rest of configuration
    }
}
```

#### 12.4.2 Test Helper Methods

**Organization and Permission Seeding:**
- `EnsureOrganizationExistsAsync`: Creates an organization if one doesn't exist
- `SeedProjectMemberPermissionsAsync`: Ensures required permissions exist and are assigned to the User role

**Usage Pattern:**
```csharp
[Fact]
public async Task TestProjectMembers()
{
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    await SeedProjectMemberPermissionsAsync(db);
    var org = await EnsureOrganizationExistsAsync(db);
    
    // Test implementation...
}
```

**Benefits:**
- Ensures test isolation
- Prevents test failures due to missing data
- Simplifies test setup by handling common prerequisites

---

## 13. Deployment

### 13.1 Docker Support

#### 13.1.1 Dockerfile

Located in `IntelliPM.API/Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IntelliPM.API/IntelliPM.API.csproj", "IntelliPM.API/"]
# ... copy and build

FROM build AS publish
RUN dotnet publish "IntelliPM.API/IntelliPM.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IntelliPM.API.dll"]
```

#### 13.1.2 Docker Compose

See `docker-compose.yml` in root directory for full stack deployment.

### 13.2 Production Checklist

- [ ] Update connection strings
- [ ] Set JWT secret key (User Secrets or environment variables)
- [ ] Configure email settings
- [ ] Set Sentry DSN
- [ ] Configure CORS origins
- [ ] Enable HTTPS
- [ ] Set up logging aggregation
- [ ] Configure health check endpoints
- [ ] Set up database backups
- [ ] Configure rate limiting
- [ ] Review security headers
- [ ] Set up monitoring and alerts

### 13.3 Environment Variables

Key environment variables:
- `ASPNETCORE_ENVIRONMENT`: `Development`, `Production`, `Testing`
- `ConnectionStrings__SqlServer`: SQL Server connection string
- `ConnectionStrings__VectorDb`: PostgreSQL connection string
- `Jwt__SecretKey`: JWT signing key
- `Sentry__Dsn`: Sentry DSN for error tracking
- `SENTRY_DSN`: Alternative Sentry DSN (takes priority)
- `SEQ_URL`: Seq server URL for centralized logging (optional)
- `SEQ_API_KEY`: Seq API key (optional)

---

## 14. API Reference

### 14.1 Authentication Endpoints

#### POST /api/v1/Auth/login

Login with username and password.

**Request:**
```json
{
  "username": "user@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "userId": 1,
  "username": "user@example.com",
  "email": "user@example.com",
  "roles": ["User"],
  "accessToken": "...",
  "refreshToken": "..."
}
```

#### POST /api/v1/Auth/register

**DEPRECATED** - Public registration is disabled. Returns `403 Forbidden`.

**Response:**
```json
{
  "error": "Public registration is disabled. Please contact your administrator for an invitation."
}
```

**Note:** Use organization invitation flow instead (`POST /api/admin/users/invite`).

#### POST /api/v1/Auth/refresh

Refresh access token using refresh token.

**Authorization:** `[AllowAnonymous]` (uses refresh token from cookie)

**Response:**
```json
{
  "message": "Token refreshed successfully"
}
```

**Note:** New access token and refresh token are set as HTTP-only cookies.

#### GET /api/v1/Auth/me

Get current authenticated user information.

**Authorization:** `[Authorize]`

**Response:**
```json
{
  "userId": 1,
  "username": "user@example.com",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "globalRole": "User",
  "organizationId": 1,
  "permissions": ["projects.view", "tasks.create"]
}
```

**Error Responses:**
- `401 Unauthorized`: Token expired or invalid, or cookie not present
- `404 Not Found`: User not found
- `500 Internal Server Error`: Error retrieving user information

**Note:** This endpoint is used by the frontend to verify authentication status and get user details on app load.

### 14.2 Project Endpoints

#### GET /api/v1/Projects

Get user's projects (paginated).

#### GET /api/v1/Projects/{id}

Get project by ID.

#### POST /api/v1/Projects

Create a new project.

**Request:**
```json
{
  "name": "My Project",
  "description": "Project description",
  "type": "Scrum",
  "sprintDurationDays": 14
}
```

#### PUT /api/v1/Projects/{id}

Update project.

#### DELETE /api/v1/Projects/{id}

Archive project (soft delete, ProductOwner only).

#### DELETE /api/v1/Projects/{id}/permanent

Permanently delete a project and all its data (ProductOwner only). This action cannot be undone.

**Authorization:** `[Authorize]` (ProductOwner role required)

**Response:**
- `204 No Content`: Project deleted successfully
- `403 Forbidden`: User doesn't have permission to delete the project
- `404 Not Found`: Project not found
- `500 Internal Server Error`: Error during deletion

**Note:** This endpoint deletes all related entities including:
- ProjectTasks, SprintItems, KPISnapshots, Sprints
- ProjectMembers, Risks, Defects, Insights, Alerts
- Activities, DocumentStores, AIDecisions, AIAgentRuns
- Tasks (Domain.Entities.Task), BacklogItems (Epics, Features, UserStories)
- Comments, Attachments, Notifications, ProjectTeams

#### GET /api/v1/Projects/{id}/my-role

Get the current user's role in a project.

**Authorization:** `[Authorize]`

**Response:**
- `200 OK`: Returns the user's `ProjectRole` or `null` if user is not a member
- `401 Unauthorized`: User not authenticated
- `500 Internal Server Error`: Error retrieving role

**Response Body:**
```json
"ProductOwner"
```
or `null` if user is not a member of the project.

#### POST /api/v1/Projects/{projectId}/assign-team

Assign a team to a project, adding all team members as project members.

**Authorization:** `[Authorize]`

**Request:**
```json
{
  "teamId": 1,
  "defaultRole": "Developer",
  "memberRoleOverrides": {
    "5": "ProductOwner",
    "7": "ScrumMaster"
  }
}
```

**Response:**
```json
{
  "projectId": 1,
  "teamId": 1,
  "assignedMembers": [
    {
      "userId": 5,
      "username": "john.doe",
      "role": "ProductOwner",
      "alreadyMember": false
    }
  ]
}
```

**Error Responses:**
- `400 Bad Request`: Validation failed
- `403 Forbidden`: User doesn't have permission to assign teams
- `404 Not Found`: Project or team not found

#### GET /api/v1/Projects/{id}/members

Get all members of a project.

**Authorization:** `[Authorize]` with `projects.view` permission

**Response:**
- `200 OK`: Returns list of project members
- `401 Unauthorized`: User not authenticated
- `403 Forbidden`: User doesn't have permission to view project members or is not a project member
- `404 Not Found`: Project not found
- `500 Internal Server Error`: Error retrieving members

**Response Body:**
```json
[
  {
    "id": 1,
    "userId": 5,
    "userName": "john.doe",
    "email": "john@example.com",
    "role": "Developer",
    "invitedAt": "2024-01-01T00:00:00Z",
    "invitedByName": "jane.admin"
  }
]
```

**Error Handling:**
- `UnauthorizedAccessException` and `UnauthorizedException` are caught and returned as `403 Forbidden`
- `NotFoundException` is returned as `404 Not Found`

#### POST /api/v1/Projects/{id}/members

Invite a member to a project.

**Authorization:** `[Authorize]` with `projects.members.invite` permission

**Request:**
```json
{
  "email": "newmember@example.com",
  "role": "Developer"
}
```

**Response:**
- `201 Created`: Member invited successfully
- `400 Bad Request`: Validation failed (invalid email, already a member, etc.)
- `401 Unauthorized`: User not authenticated
- `403 Forbidden`: User doesn't have permission to invite members
- `404 Not Found`: Project or user not found
- `500 Internal Server Error`: Error inviting member

**Response Body:**
```json
{
  "memberId": 10,
  "email": "newmember@example.com",
  "role": "Developer"
}
```

**Error Handling:**
- `ValidationException` and `InvalidOperationException` are caught and returned as `400 Bad Request`
- `UnauthorizedAccessException` and `UnauthorizedException` are caught and returned as `403 Forbidden`
- `NotFoundException` is returned as `404 Not Found`

#### PUT /api/v1/Projects/{projectId}/members/{userId}/role

Change a member's role in a project.

**Authorization:** `[Authorize]` with `projects.members.changeRole` permission

**Request:**
```json
{
  "newRole": "ScrumMaster"
}
```

**Response:**
- `204 No Content`: Role changed successfully
- `400 Bad Request`: Validation failed (cannot change ProductOwner role, invalid role, etc.)
- `401 Unauthorized`: User not authenticated
- `403 Forbidden`: User doesn't have permission to change member roles
- `404 Not Found`: Project or member not found
- `500 Internal Server Error`: Error changing role

**Error Handling:**
- `ValidationException` and `InvalidOperationException` are caught and returned as `400 Bad Request`
- `UnauthorizedAccessException` and `UnauthorizedException` are caught and returned as `403 Forbidden`
- `NotFoundException` is returned as `404 Not Found`

#### DELETE /api/v1/Projects/{projectId}/members/{userId}

Remove a member from a project.

**Authorization:** `[Authorize]` with `projects.members.remove` permission

**Response:**
- `204 No Content`: Member removed successfully
- `400 Bad Request`: Validation failed (cannot remove ProductOwner, etc.)
- `401 Unauthorized`: User not authenticated
- `403 Forbidden`: User doesn't have permission to remove members
- `404 Not Found`: Project or member not found
- `500 Internal Server Error`: Error removing member

**Error Handling:**
- `ValidationException` and `InvalidOperationException` are caught and returned as `400 Bad Request`
- `UnauthorizedAccessException` and `UnauthorizedException` are caught and returned as `403 Forbidden`
- `NotFoundException` is returned as `404 Not Found`

### 14.3 Task Endpoints

#### GET /api/v1/Tasks?projectId={id}

Get tasks by project.

#### GET /api/v1/Tasks/{id}

Get task by ID.

#### POST /api/v1/Tasks

Create a new task.

**Request:**
```json
{
  "projectId": 1,
  "title": "Implement feature X",
  "description": "Task description",
  "priority": "High",
  "storyPoints": 5
}
```

#### PUT /api/v1/Tasks/{id}

Update task.

#### PATCH /api/v1/Tasks/{id}/status

Change task status.

#### PATCH /api/v1/Tasks/{id}/assign

Assign task to user.

### 14.5 Feature Flags Endpoints

#### 14.5.1 Public Feature Flags Endpoints (All Authenticated Users)

##### GET /api/v1/feature-flags

Get all feature flags for the current user's organization.

**Authorization:** `[Authorize]` (all authenticated users)

**Query Parameters:**
- `organizationId` (optional): Filter by organization ID. If not provided, uses current user's organization.

**Response:**
```json
[
  {
    "id": "uuid",
    "name": "EnableAdvancedMetrics",
    "isEnabled": true,
    "organizationId": 1,
    "description": "Enable advanced project metrics",
    "createdAt": "2024-12-24T00:00:00Z",
    "updatedAt": "2024-12-24T00:00:00Z",
    "isGlobal": false,
    "isOrganizationSpecific": true
  }
]
```

##### GET /api/v1/feature-flags/{name}

Get a single feature flag by name.

**Authorization:** `[Authorize]` (all authenticated users)

**Path Parameters:**
- `name`: The name of the feature flag

**Query Parameters:**
- `organizationId` (optional): Filter by organization ID. If not provided, uses current user's organization.

**Response:** `200 OK` with FeatureFlagDto, or `404 Not Found` if flag doesn't exist

**Response:**
```json
{
  "id": "uuid",
  "name": "EnableAdvancedMetrics",
  "isEnabled": true,
  "organizationId": 1,
  "description": "Enable advanced project metrics",
  "createdAt": "2024-12-24T00:00:00Z",
  "updatedAt": "2024-12-24T00:00:00Z",
  "isGlobal": false,
  "isOrganizationSpecific": true
}
```

#### 14.5.2 Feature Flags Admin Endpoints (Admin Only)

##### GET /api/admin/feature-flags

Get all feature flags (optionally filtered by organization).

**Query Parameters:**
- `organizationId` (optional): Filter by organization ID

**Response:**
```json
[
  {
    "id": "uuid",
    "name": "EnableAdvancedMetrics",
    "isEnabled": true,
    "organizationId": null,
    "description": "Enable advanced project metrics",
    "createdAt": "2024-12-23T00:00:00Z",
    "updatedAt": "2024-12-23T00:00:00Z",
    "isGlobal": true,
    "isOrganizationSpecific": false
  }
]
```

##### POST /api/admin/feature-flags

Create a new feature flag.

**Request:**
```json
{
  "name": "EnableAdvancedMetrics",
  "description": "Enable advanced project metrics",
  "isEnabled": false,
  "organizationId": null
}
```

**Response:** `201 Created` with FeatureFlagDto

##### PUT /api/admin/feature-flags/{id}

Update an existing feature flag.

**Request:**
```json
{
  "isEnabled": true,
  "description": "Updated description"
}
```

**Response:** `200 OK` with FeatureFlagDto

### 14.6 Admin Endpoints

#### 14.6.1 Admin Audit Logs

##### GET /api/admin/audit-logs

Get audit logs with filters and pagination (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Page size (default: 20, max: 100)
- `action` (optional): Filter by action
- `entityType` (optional): Filter by entity type
- `userId` (optional): Filter by user ID
- `startDate` (optional): Filter by start date
- `endDate` (optional): Filter by end date

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "action": "Create",
      "entityType": "Project",
      "entityId": 123,
      "userId": 1,
      "userName": "admin@example.com",
      "timestamp": "2024-12-24T00:00:00Z",
      "details": "Created project 'My Project'"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 100,
  "totalPages": 5
}
```

#### 14.6.2 Admin System Health

##### GET /api/admin/system-health

Get current system health metrics (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Response:**
```json
{
  "database": {
    "status": "Healthy",
    "responseTime": "15ms"
  },
  "ollama": {
    "status": "Healthy",
    "endpoint": "http://localhost:11434",
    "model": "llama3.2:3b"
  },
  "memory": {
    "allocated": 524288000,
    "status": "Healthy"
  },
  "overallStatus": "Healthy"
}
```

#### 14.6.3 Admin Dead Letter Queue

##### GET /api/admin/dead-letter-queue

Get all dead letter queue messages with pagination (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Page size (default: 20, max: 100)
- `eventType` (optional): Filter by event type
- `startDate` (optional): Filter by start date
- `endDate` (optional): Filter by end date

**Response:**
```json
{
  "items": [
    {
      "id": "guid",
      "originalMessageId": "guid",
      "eventType": "UserCreatedEvent",
      "payload": "{...}",
      "originalCreatedAt": "2024-12-24T00:00:00Z",
      "movedToDlqAt": "2024-12-24T01:00:00Z",
      "totalRetryAttempts": 3,
      "lastError": "Error message",
      "idempotencyKey": "key"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 5,
  "totalPages": 1
}
```

##### POST /api/admin/dead-letter-queue/{id}/retry

Retry a dead letter message by moving it back to the Outbox (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Response:** `200 OK` with success message

##### DELETE /api/admin/dead-letter-queue/{id}

Permanently delete a dead letter message (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Response:** `204 No Content`

#### 14.6.4 Admin Dashboard

##### GET /api/admin/dashboard/stats

Get admin dashboard statistics (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Response:**
```json
{
  "totalUsers": 150,
  "activeUsers": 120,
  "inactiveUsers": 30,
  "adminCount": 5,
  "userCount": 145,
  "totalProjects": 45,
  "activeProjects": 40,
  "totalOrganizations": 10,
  "userGrowth": [
    { "month": "2025-07", "count": 0 },
    { "month": "2025-08", "count": 5 },
    { "month": "2025-09", "count": 10 }
  ],
  "recentActivities": [
    {
      "action": "Project Created",
      "userName": "admin",
      "timestamp": "2025-12-25T10:00:00Z"
    }
  ],
  "systemHealth": {
    "cpuUsage": 15.5,
    "memoryUsage": 45.2,
    "databaseStatus": "Healthy",
    "databaseResponseTimeMs": "12",
    "externalServices": {},
    "timestamp": "2025-12-25T10:00:00Z"
  }
}
```

### 14.7 Settings Endpoints

#### 14.7.1 Global Settings

##### GET /api/v1/Settings

Get all global settings or filter by category.

**Query Parameters:**
- `category` (optional): Filter by category (General, Security, Email, FeatureFlags)

**Response:**
```json
[
  {
    "key": "Application.Name",
    "value": "IntelliPM",
    "category": "General",
    "description": "Application name"
  }
]
```

##### PUT /api/v1/Settings/{key}

Update a global setting.

**Request:**
```json
{
  "value": "New Value",
  "category": "General"
}
```

##### POST /api/v1/Settings/test-email

Send a test email to verify SMTP configuration (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Request:**
```json
{
  "email": "test@example.com"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Test email sent successfully to test@example.com. Please check your inbox."
}
```

**Error Responses:**
- `400 Bad Request`: Invalid email address
- `403 Forbidden`: User is not an admin
- `500 Internal Server Error`: SMTP configuration error

### 14.8 User Management Endpoints

#### 14.8.1 Get User Projects

##### GET /api/v1/Users/{id}/projects

Get projects for a specific user (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Page size (default: 5)

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "name": "Project Name",
      "description": "Project description",
      "status": "Active"
    }
  ],
  "page": 1,
  "pageSize": 5,
  "totalCount": 10,
  "totalPages": 2
}
```

#### 14.8.2 Get User Activity

##### GET /api/v1/Users/{id}/activity

Get recent activity for a specific user (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Query Parameters:**
- `limit` (optional): Number of activities to return (default: 10)

**Response:**
```json
{
  "activities": [
    {
      "id": 1,
      "type": "Project Created",
      "userId": 1,
      "userName": "admin",
      "entityType": "Project",
      "entityId": 123,
      "entityName": "My Project",
      "projectId": 123,
      "projectName": "My Project",
      "timestamp": "2025-12-25T10:00:00Z"
    }
  ]
}
```

### 14.9 Comments Endpoints

#### POST /api/v1/Comments

Add a comment to an entity (task, project, sprint, defect).

**Authorization:** `[Authorize]`

**Request:**
```json
{
  "entityType": "Task",
  "entityId": 123,
  "content": "Great work! @john.doe can you review this?",
  "parentCommentId": null
}
```

**Response:**
```json
{
  "commentId": 456,
  "authorId": 1,
  "authorName": "jane.doe",
  "content": "Great work! @john.doe can you review this?",
  "createdAt": "2024-12-25T10:00:00Z",
  "mentionedUserIds": [2]
}
```

#### GET /api/v1/Comments?entityType={type}&entityId={id}

Get comments for an entity.

**Authorization:** `[Authorize]`

**Query Parameters:**
- `entityType`: Entity type (Task, Project, Sprint, Defect)
- `entityId`: Entity ID

**Response:**
```json
[
  {
    "id": 456,
    "authorId": 1,
    "authorName": "jane.doe",
    "content": "Great work!",
    "createdAt": "2024-12-25T10:00:00Z",
    "updatedAt": null,
    "isEdited": false,
    "parentCommentId": null,
    "replies": []
  }
]
```

#### PUT /api/v1/Comments/{id}

Update a comment.

**Authorization:** `[Authorize]` (only comment author)

**Request:**
```json
{
  "content": "Updated comment content"
}
```

#### DELETE /api/v1/Comments/{id}

Delete a comment (soft delete).

**Authorization:** `[Authorize]` (only comment author or admin)

### 14.10 Attachments Endpoints

#### POST /api/v1/Attachments/upload

Upload a file attachment.

**Authorization:** `[Authorize]`

**Request:** `multipart/form-data`
- `file`: File to upload
- `entityType`: Entity type (Task, Project, Comment, Defect)
- `entityId`: Entity ID

**Response:**
```json
{
  "id": 789,
  "fileName": "document.pdf",
  "fileSizeBytes": 1024000,
  "contentType": "application/pdf",
  "uploadedAt": "2024-12-25T10:00:00Z"
}
```

#### GET /api/v1/Attachments/{id}

Download an attachment file.

**Authorization:** `[Authorize]`

**Response:** File stream with appropriate Content-Type header

#### GET /api/v1/Attachments?entityType={type}&entityId={id}

Get all attachments for an entity.

**Authorization:** `[Authorize]`

**Response:**
```json
[
  {
    "id": 789,
    "fileName": "document.pdf",
    "fileSizeBytes": 1024000,
    "contentType": "application/pdf",
    "uploadedById": 1,
    "uploadedBy": "jane.doe",
    "uploadedAt": "2024-12-25T10:00:00Z"
  }
]
```

#### DELETE /api/v1/Attachments/{id}

Delete an attachment (soft delete).

**Authorization:** `[Authorize]` (only uploader or admin)

### 14.11 AI Governance Endpoints

#### GET /api/v1/ai/decisions

Get AI decision logs for the current organization.

**Authorization:** `[Authorize]`

**Query Parameters:**
- `decisionType` (optional): Filter by decision type
- `agentType` (optional): Filter by agent type
- `entityType` (optional): Filter by entity type
- `entityId` (optional): Filter by entity ID
- `startDate` (optional): Start date filter
- `endDate` (optional): End date filter
- `requiresApproval` (optional): Filter by approval requirement
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Page size (default: 20, max: 100)

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "decisionId": "guid",
      "decisionType": "RiskDetection",
      "agentType": "DeliveryAgent",
      "entityType": "Project",
      "entityId": 123,
      "entityName": "My Project",
      "question": "What risks exist in this project?",
      "decision": "{...}",
      "reasoning": "{...}",
      "confidenceScore": 0.85,
      "status": "Applied",
      "requiresHumanApproval": false,
      "createdAt": "2024-12-25T10:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 100,
  "totalPages": 5
}
```

#### GET /api/v1/ai/quota

Get AI quota status for the current organization.

**Authorization:** `[Authorize]`

**Response:**
```json
{
  "organizationId": 1,
  "tierName": "Pro",
  "isActive": true,
  "periodStartDate": "2024-12-01T00:00:00Z",
  "periodEndDate": "2024-12-31T23:59:59Z",
  "maxTokensPerPeriod": 1000000,
  "maxRequestsPerPeriod": 1000,
  "maxDecisionsPerPeriod": 500,
  "tokensUsed": 750000,
  "requestsUsed": 750,
  "decisionsMade": 375,
  "costAccumulated": 75.00,
  "isQuotaExceeded": false,
  "usagePercentage": 75.0
}
```

#### GET /api/v1/ai/usage-statistics

Get AI usage statistics for the current organization.

**Authorization:** `[Authorize]`

**Query Parameters:**
- `startDate` (optional): Start date for statistics
- `endDate` (optional): End date for statistics

**Response:**
```json
{
  "totalTokensUsed": 750000,
  "totalRequests": 750,
  "totalDecisions": 375,
  "totalCost": 75.00,
  "usageByAgent": {
    "DeliveryAgent": 300000,
    "ProductAgent": 250000,
    "ManagerAgent": 200000
  },
  "usageByDecisionType": {
    "RiskDetection": 200,
    "SprintPlanning": 100,
    "TaskPrioritization": 75
  }
}
```

### 14.12 Admin AI Governance Endpoints (Admin Only)

#### GET /api/admin/ai/decisions

Get all AI decision logs across all organizations (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Query Parameters:** Same as user endpoint, plus:
- `organizationId` (optional): Filter by organization

#### GET /api/admin/ai/quotas

Get all AI quotas across all organizations (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Response:**
```json
[
  {
    "organizationId": 1,
    "organizationName": "Acme Corp",
    "tierName": "Pro",
    "isActive": true,
    "tokensUsed": 750000,
    "maxTokensPerPeriod": 1000000,
    "isQuotaExceeded": false
  }
]
```

#### PUT /api/admin/ai/quota/{organizationId}

Update AI quota for an organization (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Request:**
```json
{
  "tierName": "Enterprise",
  "maxTokensPerPeriod": 10000000,
  "maxRequestsPerPeriod": 10000,
  "maxDecisionsPerPeriod": 5000,
  "allowOverage": true,
  "overageRate": 0.00001
}
```

#### POST /api/admin/ai/disable/{organizationId}

Disable AI features for an organization (kill switch) (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Response:**
```json
{
  "success": true,
  "message": "AI features disabled for organization"
}
```

#### POST /api/admin/ai/enable/{organizationId}

Enable AI features for an organization (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

#### GET /api/admin/ai/decisions/export

Export AI decisions to CSV (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Query Parameters:** Same filters as GET endpoint

**Response:** CSV file download

### 14.12.1 Admin Member Permissions Endpoints

#### GET /api/admin/permissions/members

Get a paginated list of organization members with their permissions (Admin only - own organization).

**Authorization:** `[Authorize(Roles = "Admin,SuperAdmin")]`

**Query Parameters:**
- `page` (int, default: 1): Page number
- `pageSize` (int, default: 20, max: 100): Page size
- `searchTerm` (string, optional): Search by name or email

**Response:**
```json
{
  "items": [
    {
      "userId": 1,
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "fullName": "John Doe",
      "globalRole": "User",
      "organizationId": 1,
      "organizationName": "Acme Corp",
      "permissions": ["projects.view", "tasks.create"],
      "permissionIds": [1, 5]
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 50,
  "totalPages": 3
}
```

**Features:**
- Returns only members from the current user's organization (tenant isolation)
- Permissions are derived from the user's `GlobalRole` via `RolePermission` table
- Search functionality filters by email, first name, last name, or username

#### PUT /api/admin/permissions/members/{userId}

Update a member's role and/or permissions (Admin only - own organization).

**Authorization:** `[Authorize(Roles = "Admin,SuperAdmin")]`

**Request:**
```json
{
  "globalRole": "Admin",  // Optional: update role (User or Admin only)
  "permissionIds": [1, 2, 3]  // Optional: explicit permission IDs
}
```

**Response:**
```json
{
  "userId": 1,
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "fullName": "John Doe",
  "globalRole": "Admin",
  "organizationId": 1,
  "organizationName": "Acme Corp",
  "permissions": ["projects.create", "projects.edit", "users.view"],
  "permissionIds": [1, 2, 3]
}
```

**Validation:**
- Admin cannot assign SuperAdmin role
- Admin can only assign Admin or User roles
- Admin cannot change their own role/permissions (prevents lockout)
- Assigned permissions must be subset of organization's allowed permissions (policy enforcement)
- Tenant isolation: Admin can only modify users in their own organization

**Error Responses:**
- `400 Bad Request`: Validation failed or permissions not allowed by organization policy
- `403 Forbidden`: User doesn't have permission or trying to change own permissions
- `404 Not Found`: User not found or not in same organization

### 14.12.2 SuperAdmin Permission Policy Endpoints

#### GET /api/v1/superadmin/organizations/{orgId}/permission-policy

Get organization permission policy by organization ID (SuperAdmin only).

**Authorization:** `[RequireSuperAdmin]`

**Note:** SuperAdmin routes use API versioning: `/api/v1/superadmin/organizations/...`

**Response:**
```json
{
  "id": 1,
  "organizationId": 1,
  "organizationName": "Acme Corp",
  "organizationCode": "acme-corp",
  "allowedPermissions": ["projects.create", "projects.edit", "users.view"],
  "isActive": true,
  "createdAt": "2025-01-02T10:00:00Z",
  "updatedAt": "2025-01-02T11:00:00Z"
}
```

**Note:** If no policy exists, returns default policy with `id: 0` and empty `allowedPermissions` array (indicating all permissions allowed).

#### PUT /api/v1/superadmin/organizations/{orgId}/permission-policy

Upsert (create or update) organization permission policy (SuperAdmin only).

**Authorization:** `[RequireSuperAdmin]`

**Note:** SuperAdmin routes use API versioning: `/api/v1/superadmin/organizations/...`

**Request:**
```json
{
  "allowedPermissions": ["projects.create", "projects.edit", "users.view"],
  "isActive": true
}
```

**Response:**
```json
{
  "id": 1,
  "organizationId": 1,
  "organizationName": "Acme Corp",
  "organizationCode": "acme-corp",
  "allowedPermissions": ["projects.create", "projects.edit", "users.view"],
  "isActive": true,
  "createdAt": "2025-01-02T10:00:00Z",
  "updatedAt": "2025-01-02T11:00:00Z"
}
```

**Validation:**
- All permission names must exist in the system
- Invalid permissions return `400 Bad Request` with list of invalid permissions
- Creates policy if not exists, updates if exists (upsert operation)

**Error Responses:**
- `400 Bad Request`: Invalid permissions specified
- `403 Forbidden`: User is not SuperAdmin
- `404 Not Found`: Organization not found

### 14.12.3 SuperAdmin AI Quota Endpoints

#### GET /api/v1/superadmin/organizations/{orgId}/ai-quota

Get organization AI quota by organization ID (SuperAdmin only).

**Authorization:** `[RequireSuperAdmin]`

**Response:**
```json
{
  "id": 1,
  "organizationId": 1,
  "organizationName": "Acme Corp",
  "organizationCode": "acme-corp",
  "monthlyTokenLimit": 1000000,
  "monthlyRequestLimit": 1000,
  "resetDayOfMonth": 1,
  "isAIEnabled": true,
  "createdAt": "2025-01-02T10:00:00Z",
  "updatedAt": "2025-01-02T11:00:00Z"
}
```

**Note:** SuperAdmin routes use API versioning: `/api/v1/superadmin/organizations/...`

#### PUT /api/v1/superadmin/organizations/{orgId}/ai-quota

Upsert (create or update) organization AI quota (SuperAdmin only).

**Authorization:** `[RequireSuperAdmin]`

**Request:**
```json
{
  "monthlyTokenLimit": 2000000,
  "monthlyRequestLimit": 2000,
  "resetDayOfMonth": 1,
  "isAIEnabled": true
}
```

**Response:** `200 OK` with updated organization AI quota

**Note:** SuperAdmin routes use API versioning: `/api/v1/superadmin/organizations/...`

#### GET /api/v1/superadmin/organizations/ai-quotas

Get a paginated list of all organization AI quotas (SuperAdmin only).

**Authorization:** `[RequireSuperAdmin]`

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Page size (default: 20, max: 100)
- `searchTerm` (optional): Search by organization name or code
- `isAIEnabled` (optional): Filter by AI enabled status

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "organizationId": 1,
      "organizationName": "Acme Corp",
      "organizationCode": "acme-corp",
      "monthlyTokenLimit": 1000000,
      "monthlyRequestLimit": 1000,
      "isAIEnabled": true
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 10,
  "totalPages": 1
}
```

**Note:** SuperAdmin routes use API versioning: `/api/v1/superadmin/organizations/...`

### 14.13 Agent Endpoints

#### POST /api/v1/Agent/improve-task

Improves a task description using AI with automatic function calling.

**Authorization:** `[Authorize]`

**Request:**
```json
{
  "description": "Fix bug in login page"
}
```

**Response:**
```json
{
  "status": "Success",
  "content": "{...improved task data...}",
  "executionTimeMs": 1234,
  "createdAt": "2024-12-31T10:00:00Z"
}
```

**Validation:**
- Description cannot be empty
- Description max length: 5000 characters

**Error Responses:**
- `400 Bad Request`: Description is empty or too long
- `401 Unauthorized`: User not authenticated
- `500 Internal Server Error`: Error during task improvement

#### POST /api/v1/Agent/generate-retrospective/{sprintId}

Generate sprint retrospective using AI agent.

**Authorization:** `[Authorize]`

**Parameters:**
- `sprintId`: Sprint ID (path parameter)

**Response:**
```json
{
  "status": "Success",
  "content": "{...retrospective data...}",
  "executionTimeMs": 4500,
  "createdAt": "2024-12-31T10:00:00Z"
}
```

**Validation:**
- Sprint must exist
- Sprint must be completed (Status = "Completed")
- Retrospective notes are saved to Sprint.RetrospectiveNotes

**Error Responses:**
- `400 Bad Request`: Sprint is not completed
- `404 Not Found`: Sprint not found
- `500 Internal Server Error`: Error during retrospective generation

**Note:** Uses SprintRetrospectivePlugin with Semantic Kernel to gather sprint metrics, completed/incomplete tasks, defects, and team activity.

### 14.14 Read Models Endpoints

#### GET /api/v1/read-models/task-board/{projectId}

Get task board read model for a project.

**Authorization:** `[Authorize]`

**Response:**
```json
{
  "projectId": 123,
  "todoTasks": [...],
  "inProgressTasks": [...],
  "doneTasks": [...],
  "lastUpdated": "2024-12-25T10:00:00Z"
}
```

#### GET /api/v1/read-models/sprint-summary/{sprintId}

Get sprint summary read model.

**Authorization:** `[Authorize]`

**Response:**
```json
{
  "sprintId": 456,
  "totalStoryPoints": 50,
  "completedStoryPoints": 30,
  "tasksCompleted": 15,
  "tasksInProgress": 5,
  "tasksTodo": 10,
  "velocity": 30,
  "lastUpdated": "2024-12-25T10:00:00Z"
}
```

#### GET /api/v1/read-models/project-overview/{projectId}

Get project overview read model.

**Authorization:** `[Authorize]`

**Response:**
```json
{
  "projectId": 123,
  "activeSprints": 2,
  "totalBacklogItems": 50,
  "teamMembers": 10,
  "lastUpdated": "2024-12-25T10:00:00Z"
}
```

#### POST /api/admin/read-models/rebuild

Rebuild all read models (Admin only).

**Authorization:** `[Authorize(Roles = "Admin")]`

**Request:**
```json
{
  "readModelType": "All" // or "TaskBoard", "SprintSummary", "ProjectOverview"
}
```

### 14.15 Releases Endpoints

#### GET /api/v1/projects/{projectId}/releases

Get all releases for a project with optional status filtering.

**Authorization:** `[RequirePermission("releases.view")]`

**Query Parameters:**
- `status` (optional): Filter by release status (Planned, InProgress, Testing, ReadyForDeployment, Deployed, Failed, Cancelled)

**Response:**
```json
[
  {
    "id": 1,
    "projectId": 123,
    "name": "v2.1.0",
    "version": "2.1.0",
    "description": "Major feature release",
    "plannedDate": "2024-12-31T00:00:00Z",
    "actualReleaseDate": null,
    "status": "Planned",
    "type": "Major",
    "isPreRelease": false,
    "tagName": "v2.1.0",
    "sprintCount": 3,
    "completedTasksCount": 45,
    "totalTasksCount": 50,
    "overallQualityStatus": null,
    "createdAt": "2024-12-01T00:00:00Z",
    "createdByName": "admin",
    "releasedByName": null
  }
]
```

#### GET /api/v1/releases/{id}

Get a specific release by ID with full details including sprints and quality gates.

**Authorization:** `[RequirePermission("releases.view")]`

**Response:**
```json
{
  "id": 1,
  "projectId": 123,
  "name": "v2.1.0",
  "version": "2.1.0",
  "description": "Major feature release",
  "plannedDate": "2024-12-31T00:00:00Z",
  "actualReleaseDate": null,
  "status": "Planned",
  "type": "Major",
  "releaseNotes": null,
  "changeLog": null,
  "isPreRelease": false,
  "tagName": "v2.1.0",
  "sprintCount": 3,
  "completedTasksCount": 45,
  "totalTasksCount": 50,
  "overallQualityStatus": null,
  "qualityGates": [],
  "sprints": [
    {
      "id": 10,
      "name": "Sprint 1",
      "startDate": "2024-12-01T00:00:00Z",
      "endDate": "2024-12-14T00:00:00Z",
      "status": "Completed",
      "completedTasksCount": 15,
      "totalTasksCount": 16,
      "completionPercentage": 94
    }
  ],
  "createdAt": "2024-12-01T00:00:00Z",
  "createdByName": "admin",
  "releasedByName": null
}
```

#### GET /api/v1/projects/{projectId}/releases/statistics

Get release statistics for a project.

**Authorization:** `[RequirePermission("releases.view")]`

**Response:**
```json
{
  "totalReleases": 10,
  "plannedReleases": 2,
  "inProgressReleases": 3,
  "completedReleases": 4,
  "cancelledReleases": 1,
  "upcomingRelease": {
    "id": 5,
    "name": "v2.2.0",
    "version": "2.2.0",
    "plannedDate": "2025-01-15T00:00:00Z",
    "status": "Planned",
    "type": "Minor"
  }
}
```

#### GET /api/v1/projects/{projectId}/sprints/available

Get available sprints that can be added to a release.

**Authorization:** `[RequirePermission("sprints.view")]`

**Response:**
```json
[
  {
    "id": 15,
    "name": "Sprint 5",
    "status": "Completed",
    "startDate": "2024-12-15T00:00:00Z",
    "endDate": "2024-12-28T00:00:00Z"
  }
]
```

#### POST /api/v1/projects/{projectId}/releases

Create a new release for a project.

**Authorization:** `[RequirePermission("releases.create")]`

**Request:**
```json
{
  "name": "v2.1.0",
  "version": "2.1.0",
  "description": "Major feature release",
  "type": "Major",
  "plannedDate": "2024-12-31T00:00:00Z",
  "isPreRelease": false,
  "tagName": "v2.1.0",
  "sprintIds": [10, 11, 12]
}
```

**Response:** `201 Created` with release details

#### PUT /api/v1/releases/{id}

Update an existing release.

**Authorization:** `[RequirePermission("releases.edit")]`

**Request:**
```json
{
  "name": "v2.1.0",
  "version": "2.1.0",
  "description": "Updated description",
  "plannedDate": "2025-01-15T00:00:00Z",
  "status": "InProgress"
}
```

**Response:** `200 OK` with updated release

#### DELETE /api/v1/releases/{id}

Delete a release (cannot delete deployed releases).

**Authorization:** `[RequirePermission("releases.delete")]`

**Response:** `204 No Content`

#### POST /api/v1/releases/{id}/deploy

Deploy a release (requires all quality gates to pass).

**Authorization:** `[RequirePermission("releases.deploy")]`

**Response:** `200 OK` with deployed release

#### POST /api/v1/releases/{releaseId}/sprints/{sprintId}

Add a sprint to a release.

**Authorization:** `[RequirePermission("releases.edit")]`

**Response:** `200 OK`

#### POST /api/v1/releases/{releaseId}/sprints/bulk

Bulk add sprints to a release.

**Authorization:** `[RequirePermission("releases.edit")]`

**Request:**
```json
{
  "sprintIds": [10, 11, 12]
}
```

**Response:** `200 OK`

#### DELETE /api/v1/releases/sprints/{sprintId}

Remove a sprint from a release.

**Authorization:** `[RequirePermission("releases.edit")]`

**Response:** `204 No Content`

#### POST /api/v1/releases/{releaseId}/notes/generate

Generate release notes using AI.

**Authorization:** `[RequirePermission("releases.notes.edit")]`

**Response:**
```json
{
  "releaseNotes": "# Release Notes v2.1.0\n\n## New Features\n- Feature A\n- Feature B"
}
```

#### PUT /api/v1/releases/{releaseId}/notes

Update release notes manually.

**Authorization:** `[RequirePermission("releases.notes.edit")]`

**Request:**
```json
{
  "releaseNotes": "# Updated Release Notes",
  "autoGenerate": false
}
```

**Response:** `200 OK`

#### POST /api/v1/releases/{releaseId}/changelog/generate

Generate changelog using AI.

**Authorization:** `[RequirePermission("releases.notes.edit")]`

**Response:**
```json
{
  "changeLog": "## Changelog\n\n### Added\n- Feature A\n- Feature B"
}
```

#### PUT /api/v1/releases/{releaseId}/changelog

Update changelog manually.

**Authorization:** `[RequirePermission("releases.notes.edit")]`

**Request:**
```json
{
  "changeLog": "## Updated Changelog",
  "autoGenerate": false
}
```

**Response:** `200 OK`

#### POST /api/v1/releases/{releaseId}/quality-gates/evaluate

Evaluate quality gates for a release.

**Authorization:** `[RequirePermission("releases.view")]`

**Response:** `200 OK` with quality gate evaluation results

#### POST /api/v1/releases/{releaseId}/quality-gates/approve

Approve a quality gate.

**Authorization:** `[RequirePermission("releases.quality-gates.approve")]`

**Request:**
```json
{
  "gateType": 1
}
```

**Response:** `200 OK`

### 14.15 Organization User Invitation Endpoints (Admin Only)

#### POST /api/admin/users/invite

Invite a user to join the organization.

**Authorization:** `[Authorize(Roles = "Admin")]`

**Request:**
```json
{
  "email": "newuser@example.com",
  "role": "User",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response:**
```json
{
  "invitationId": "guid",
  "email": "newuser@example.com",
  "invitationLink": "http://localhost:3000/invite/accept/token123"
}
```

**Error Responses:**
- `400 Bad Request`: Validation failed
- `403 Forbidden`: User is not an admin
- `409 Conflict`: User already exists or pending invitation exists
- `500 Internal Server Error`: Error sending invitation email (invitation is still created)

**Note:** The invitation is created even if the email fails to send. The invitation link can still be used manually.

#### POST /api/v1/Auth/invite/accept (Organization Invitation)

Accept an organization invitation and create user account.

**Authorization:** `[AllowAnonymous]`

**Request:**
```json
{
  "token": "invitation-token",
  "username": "johndoe",
  "password": "SecurePass123",
  "confirmPassword": "SecurePass123"
}
```

**Response:**
```json
{
  "userId": 1,
  "username": "johndoe",
  "email": "newuser@example.com",
  "accessToken": "...",
  "refreshToken": "..."
}
```

**Validation Requirements:**
- Token: Required
- Username: Required, 3-50 characters, alphanumeric + underscore
- Password: Required, minimum 8 characters, must contain uppercase + lowercase + number
- ConfirmPassword: Must match Password

**Error Responses:**
- `400 Bad Request`: Validation failed or passwords don't match
- `404 Not Found`: Invalid or expired invitation token
- `409 Conflict`: Username already taken or user already exists

### 14.7 Common Response Formats

#### Success Response

```json
{
  "data": { ... },
  "statusCode": 200
}
```

#### Error Response

```json
{
  "error": "Error message",
  "errors": {
    "field": ["Validation error"]
  },
  "statusCode": 400
}
```

#### Paginated Response

```json
{
  "data": [ ... ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 100,
  "totalPages": 5
}
```

---

## 15. Troubleshooting

### 15.1 Common Issues

#### 15.1.1 Database Connection Errors

**Issue**: Cannot connect to SQL Server/PostgreSQL

**Solutions:**
- Verify connection strings in `appsettings.json`
- Check database server is running
- Verify firewall rules
- Check credentials

#### 15.1.2 JWT Token Errors

**Issue**: "JWT SecretKey must be configured"

**Solutions:**
- Set JWT secret key in User Secrets or environment variables
- Ensure secret key is at least 32 characters
- Restart application after configuration change

#### 15.1.3 Ollama Connection Errors

**Issue**: Cannot connect to Ollama

**Solutions:**
- Verify Ollama is running: `ollama serve`
- Check endpoint in configuration: `Ollama:Endpoint`
- Verify model is available: `ollama list`

#### 15.1.4 Migration Errors

**Issue**: Migrations fail to apply

**Solutions:**
- Check database connection
- Verify user has CREATE/ALTER permissions
- Review migration SQL for conflicts
- Manually apply migrations if needed

#### 15.1.5 Permission Errors

**Issue**: "You don't have permission" errors

**Solutions:**
- Verify user's `GlobalRole` or `ProjectRole`
- Check `RolePermission` mappings in database
- Verify permission names match exactly
- Clear permission cache if using caching

#### 15.1.6 Feature Flag Errors

**Issue**: Feature returns disabled unexpectedly

**Solutions:**
- Check if feature flag exists in database
- Verify OrganizationId matches (null = global)
- Cache expires after 5 minutes; wait or restart app
- Check `FeatureFlags` table for correct `IsEnabled` value

#### 15.1.7 Outbox Processing Errors

**Issue**: Domain events not being processed

**Solutions:**
- Check `OutboxMessages` table for unprocessed messages
- Verify `ProcessedAt` is NULL and `RetryCount` < 3
- Check `NextRetryAt` for scheduled retries
- Review `Error` column for failure details
- Ensure `OutboxProcessor` is registered and running

#### 15.1.8 User Invitation Errors

**Issue**: 500 Internal Server Error when inviting users

**Solutions:**
- Check SMTP configuration in GlobalSettings (Email category)
- Verify email template exists: `EmailTemplates/OrganizationInvitation.html`
- Check logs for email service errors (invitation is still created even if email fails)
- Verify `Frontend:BaseUrl` configuration for invitation links
- Check that `IEmailService` is properly registered in DI container

#### 15.1.9 Admin Dashboard 500 Errors

**Issue**: `/api/admin/dashboard/stats` returns 500 Internal Server Error

**Solutions:**
- Check logs for specific error in `GetAdminDashboardStatsQueryHandler`
- Verify `CurrentUserService.GetOrganizationId()` returns valid organization ID
- Check that all required entities (User, Project, Activity) are accessible
- Verify `SystemHealth` query doesn't fail (has fallback to default values)
- Check that `RecentActivities` query handles empty project lists correctly

#### 15.1.10 Project Permanent Deletion 500 Errors

**Issue**: `DELETE /api/v1/Projects/{id}/permanent` returns 500 Internal Server Error

**Solutions:**
- Check logs for specific error in `DeleteProjectCommandHandler`
- Verify all related entities are properly deleted in correct order:
  1. ProjectTasks
  2. SprintItems, KPISnapshots, Sprints
  3. ProjectMembers, Risks, Defects, Insights, Alerts, Activities
  4. DocumentStores, ProjectTeams, Notifications
  5. AIDecisions, AIAgentRuns
  6. Tasks (Domain.Entities.Task), BacklogItems
  7. Comments and Attachments (polymorphic)
  8. Project itself
- Check for foreign key constraint violations
- Verify navigation properties are properly loaded before deletion
- Ensure null-safety checks are in place for navigation properties

#### 15.1.11 Authentication 401 Errors

**Issue**: `GET /api/v1/Auth/me` returns 401 Unauthorized

**Solutions:**
- Verify `auth_token` cookie is present in request (check browser DevTools)
- Check cookie expiration (15 minutes for access token)
- Verify CORS configuration allows credentials (`AllowCredentials: true`)
- Check that cookie domain and path match frontend origin
- Verify JWT secret key is correctly configured
- Check middleware logs for cookie status
- Try refreshing token using `POST /api/v1/Auth/refresh`
- Clear cookies and re-login if token is expired

### 15.2 Debugging

#### 15.2.1 Enable Detailed Logging

Update `appsettings.Development.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "System": "Information"
      }
    }
  }
}
```

#### 15.2.2 Database Logging

Enable EF Core logging:

```csharp
options.UseSqlServer(connectionString, sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure(3);
    sqlOptions.LogTo(Console.WriteLine, LogLevel.Information);
});
```

### 15.3 Performance Issues

#### 15.3.1 Slow Queries

- Enable query logging to identify slow queries
- Add missing indexes
- Review N+1 query problems
- Use `AsNoTracking()` for read-only queries

#### 15.3.2 High Memory Usage

- Review caching strategy
- Check for memory leaks in services
- Monitor health check endpoints
- Review connection pooling settings

---

## 16. Best Practices

### 16.1 Code Organization

- Follow Clean Architecture principles
- Keep handlers focused and single-purpose
- Use dependency injection
- Implement proper error handling

### 16.2 Security

- Always validate user permissions
- Use parameterized queries (EF Core handles this)
- Never expose sensitive data in responses
- Implement proper logging (without PII)

### 16.3 Performance

- Use async/await for I/O operations
- Implement caching where appropriate
- Use pagination for large datasets
- Optimize database queries

### 16.4 Testing

- Write unit tests for business logic
- Write integration tests for API endpoints
- Test permission checks
- Test error scenarios

---

## 17. Future Improvements

### 17.1 Architecture

- [x] Implement Domain Events (IDomainEvent, DomainEventDispatcher)
- [x] Implement Outbox Pattern for reliable event publishing
- [x] Implement Dead Letter Queue for failed messages
- [x] Implement Feature Flags for dynamic feature toggles
- [x] Implement ProjectTeam entity for explicit team-to-project relationships
- [x] Implement UserCreatedEvent and UserUpdatedEvent with automatic auditing
- [ ] Move AI handlers to Application layer
- [ ] Add authorization policy handlers
- [ ] Implement event sourcing (if needed)

### 17.2 Security

- [x] Add `LastLoginAt` tracking for user authentication monitoring
- [x] Implement comprehensive settings management (General, Security, Email)
- [x] Add password policy configuration
- [x] Implement test email functionality for SMTP verification
- [ ] Add permission checks to all query handlers
- [ ] Implement authorization policy handlers
- [ ] Add OrganizationId to missing entities
- [x] Implement audit logging (AuditLog entity and endpoints)

### 17.3 Features

- [x] User detail endpoints (projects, activity history)
- [x] Global settings management with categories
- [x] Email configuration and testing
- [x] Security settings configuration
- [x] Last login tracking
- [ ] Real-time notifications (SignalR)
- [ ] File upload support
- [ ] Advanced search with Elasticsearch
- [ ] Export functionality (PDF, Excel)
- [ ] Webhook support

### 17.4 Performance

- [ ] Implement Redis for distributed caching
- [ ] Add database read replicas
- [ ] Implement response compression
- [ ] Add CDN for static assets

---

## 18. Contributing

### 18.1 Code Standards

- Follow existing code style
- Add XML documentation
- Write unit tests
- Update this documentation

### 18.2 Pull Request Process

1. Create feature branch
2. Implement changes
3. Write/update tests
4. Update documentation
5. Submit pull request

---

## 19. Support

### 19.1 Documentation

- This document
- Code comments
- API Swagger documentation
- Migration guides in `Scripts/` folder

### 19.2 Issues

Report issues via:
- GitHub Issues
- Internal ticketing system
- Team communication channels

---

## 20. Missing Features

Based on comprehensive audit (December 2024), the following features are identified as missing from the current implementation:

### 20.1 Critical Missing Entities

#### 20.1.1 TaskDependency (Priority: 🔴 CRITICAL)

**Status:** ✅ **Partially Implemented (v2.9)**

**Impact:** High - Essential for project management workflow

**What's Implemented:**
- ✅ Entity `TaskDependency` for explicit task relationships
- ✅ Frontend components for dependency management (`TaskDependenciesList`, `AddDependencyDialog`, `DependencyGraph`)
- ✅ Frontend API client (`dependencies.ts`) with dependency management functions
- ✅ Hooks for task dependencies (`useTaskDependencies`, `useProjectTaskDependencies`)

**What's Missing:**
- ⚠️ Backend API endpoints for task dependencies (frontend expects endpoints but they may not be fully implemented)
- ⚠️ Cycle detection algorithm for dependency validation
- ⚠️ Dependency graph visualization API endpoint
- ⚠️ Automatic blocker detection when dependencies change status

**Current Status:**
- Entity exists in domain layer
- Frontend is ready for dependency management
- Backend endpoints need verification/implementation

**Estimated Remaining Work:** 2-3 days

#### 20.1.2 Milestone (Priority: 🟠 HIGH)

**Status:** ✅ **Implemented (v2.9)**

**Impact:** Medium - Important for advanced planning

**What's Implemented:**
- ✅ Entity `Milestone` for project milestones with `MilestoneStatus` and `MilestoneType` enums
- ✅ `MilestonesController` with full CRUD operations
- ✅ API endpoints: `GET /api/v1/projects/{id}/milestones`, `POST /api/v1/projects/{id}/milestones`
- ✅ Milestone statistics and next milestone queries
- ✅ Domain events: `MilestoneCreatedEvent`, `MilestoneCompletedEvent`, `MilestoneMissedEvent`
- ✅ Frontend components for milestone management

**Remaining Work:**
- Calendar view integration
- Gantt chart integration

#### 20.1.3 Release (Priority: 🟠 HIGH)

**Status:** ✅ **Implemented (v2.9)**

**Impact:** Medium - Essential for release management

**What's Implemented:**
- ✅ Entity `Release` for version/release management with `ReleaseStatus` and `ReleaseType` enums
- ✅ Entity `QualityGate` for quality checks
- ✅ Release notes generation
- ✅ Quality gates checking
- ✅ Release planning (linking sprints to releases via `Sprint.ReleaseId`)
- ✅ Domain events: `ReleaseNotesGeneratedEvent`, `QualityGatesEvaluatedEvent`
- ✅ Frontend components for release management

**What's Implemented:**
- ✅ `ReleasesController` with 17 endpoints for complete release management
- ✅ All CQRS handlers, commands, queries, and validators (44 files)
- ✅ Release CRUD operations (Create, Read, Update, Delete)
- ✅ Release deployment with quality gate validation
- ✅ Sprint management (add, remove, bulk operations)
- ✅ AI-powered release notes and changelog generation
- ✅ Quality gates evaluation and approval
- ✅ Release statistics and available sprints queries
- ✅ All endpoints secured with `[RequirePermission]` attributes
- ✅ Multi-tenancy support via `OrganizationId` filtering
- ✅ Frontend components for release management (18 components)

**API Endpoints:**
- `GET /api/v1/projects/{projectId}/releases` - List releases
- `GET /api/v1/releases/{id}` - Get release by ID
- `GET /api/v1/projects/{projectId}/releases/statistics` - Get statistics
- `GET /api/v1/projects/{projectId}/sprints/available` - Get available sprints
- `POST /api/v1/projects/{projectId}/releases` - Create release
- `PUT /api/v1/releases/{id}` - Update release
- `DELETE /api/v1/releases/{id}` - Delete release
- `POST /api/v1/releases/{id}/deploy` - Deploy release
- `POST /api/v1/releases/{releaseId}/sprints/{sprintId}` - Add sprint
- `POST /api/v1/releases/{releaseId}/sprints/bulk` - Bulk add sprints
- `DELETE /api/v1/releases/sprints/{sprintId}` - Remove sprint
- `POST /api/v1/releases/{releaseId}/notes/generate` - Generate release notes
- `PUT /api/v1/releases/{releaseId}/notes` - Update release notes
- `POST /api/v1/releases/{releaseId}/changelog/generate` - Generate changelog
- `PUT /api/v1/releases/{releaseId}/changelog` - Update changelog
- `POST /api/v1/releases/{releaseId}/quality-gates/evaluate` - Evaluate quality gates
- `POST /api/v1/releases/{releaseId}/quality-gates/approve` - Approve quality gate

#### 20.1.4 TestCase & TestExecution (Priority: 🟡 MEDIUM)

**Status:** ❌ **Not Implemented**

**Impact:** Medium - Important for QA workflow

**What's Missing:**
- Entity `TestCase` for test case management
- Entity `TestExecution` for test execution tracking
- Test coverage calculation
- Quality gates based on test results

**Current Workaround:**
- `Defect` entity exists for bug tracking
- `QAAgent` can analyze defects
- No test case or execution tracking

**Estimated Implementation:** 6-9 days

**Roadmap:**
1. Create `TestCase` and `TestExecution` entities
2. Create API endpoints: `GET /api/v1/Projects/{id}/test-cases`, `POST /api/v1/TestCases/{id}/execute`
3. Implement test coverage calculation
4. Integrate with Release quality gates

### 20.2 API Endpoints Status

#### 20.2.1 Notifications

**Status:** ✅ **All Endpoints Implemented**
- ✅ `GET /api/v1/Notifications/unread-count` - **Implemented**
  - Returns `GetUnreadNotificationCountResponse` with `unreadCount` property
  - Dedicated endpoint for efficient unread count retrieval
  - Used by frontend notification bell for badge display

#### 20.2.2 Comments & Attachments

**Status:** ✅ **Implemented (v2.6)**

**Endpoints Added:**
- ✅ `GET /api/v1/Comments?entityType={type}&entityId={id}` - Get comments
- ✅ `POST /api/v1/Comments` - Add comment
- ✅ `PUT /api/v1/Comments/{id}` - Update comment
- ✅ `DELETE /api/v1/Comments/{id}` - Delete comment
- ✅ `GET /api/v1/Attachments?entityType={type}&entityId={id}` - Get attachments
- ✅ `POST /api/v1/Attachments/upload` - Upload attachment
- ✅ `GET /api/v1/Attachments/{id}` - Download attachment
- ✅ `DELETE /api/v1/Attachments/{id}` - Delete attachment

### 20.3 Implementation Status Summary

| Feature | Status | Priority | Estimated Effort |
|---------|--------|----------|------------------|
| **TaskDependency** | ✅ Partial | 🔴 CRITICAL | 2-3 days remaining |
| **Milestone** | ✅ Implemented | 🟠 HIGH | - |
| **Release** | ✅ Implemented | 🟠 HIGH | - |
| **TestCase** | ❌ Missing | 🟡 MEDIUM | 6-9 days |
| **TestExecution** | ❌ Missing | 🟡 MEDIUM | (included above) |
| **Comments API** | ✅ Implemented | - | - |
| **Attachments API** | ✅ Implemented | - | - |
| **Notifications/unread-count** | ✅ Implemented | - | - |

**Total Estimated Effort:** 8-12 days remaining (unread-count endpoint now implemented)

---

## 21. API Endpoints Audit

### 21.1 Endpoints Real vs Documented

This section documents the actual API endpoints available in the backend compared to what's documented in Swagger and this documentation.

#### 21.1.1 Controllers Summary

**Total Controllers:** 42 controllers (26 standard + 14 admin + 2 superadmin + 1 DEBUG-only TestController)

| Controller | Route Pattern | Endpoints | Status | Notes |
|------------|---------------|-----------|--------|
| `ProjectsController` | `/api/v1/Projects` | 13 | ✅ Documented | All endpoints have `[RequirePermission]` |
| `TasksController` | `/api/v1/Tasks` | 11 | ✅ Documented | All endpoints have `[RequirePermission]` |
| `SprintsController` | `/api/v1/Sprints` | 8 | ✅ Documented | All endpoints have `[RequirePermission]` |
| `TeamsController` | `/api/v1/Teams` | 5 | ✅ Documented |
| `DefectsController` | `/api/v1/Defects` | 5 | ✅ Documented |
| `UsersController` | `/api/v1/Users` | 6 | ✅ Documented |
| `AuthController` | `/api/v1/Auth` | 10 | ✅ Documented | Includes login, register (deprecated), refresh, logout, getMe, validateInvite, acceptInvite |
| `NotificationsController` | `/api/v1/Notifications` | 4 | ✅ Documented |
| `CommentsController` | `/api/v1/Comments` | 4 | ✅ Documented (v2.6) |
| `AttachmentsController` | `/api/v1/Attachments` | 4 | ✅ Documented (v2.6) |
| `MetricsController` | `/api/v1/Metrics` | 4 | ✅ Documented |
| `AgentsController` | `/api/v1/projects/{id}/agents` | 6 | ✅ Documented (all endpoints require `projects.view` permission) |
| `AgentController` | `/api/v1/Agent` | 9 | ✅ Documented | Includes improve-task, analyze-project, detect-risks, plan-sprint, analyze-dependencies, metrics, audit-logs, generate-retrospective |
| `SearchController` | `/api/v1/Search` | 1 | ✅ Documented |
| `SettingsController` | `/api/v1/Settings` | 3 | ✅ Documented |
| `PermissionsController` | `/api/v1/Permissions` | 2 | ✅ Documented |
| `BacklogController` | `/api/v1/Backlog` | 4 | ✅ Documented |
| `AlertsController` | `/api/v1/Alerts` | 2 | ✅ Documented |
| `InsightsController` | `/api/v1/Insights` | 1 | ✅ Documented |
| `ActivityController` | `/api/v1/Activity` | 1 | ✅ Documented |
| `HealthController` | `/api/v1/Health` | 1 | ✅ Documented |
| `HealthApiController` | `/api/health/api` | 1 | ✅ Documented (smoke tests) |
| `FeatureFlagsController` | `/api/v1/feature-flags` | 2 | ✅ Documented |
| `ReadModelsController` | `/api/v1/ReadModels` | 3 | ✅ Documented |
| `AIGovernanceController` | `/api/v1/AIGovernance` | 3 | ✅ Documented |
| `Admin/DashboardController` | `/api/admin/dashboard` | 1 | ✅ Documented |
| `Admin/UsersController` | `/api/admin/users` | 3 | ✅ Documented |
| `Admin/FeatureFlagsController` | `/api/admin/feature-flags` | 4 | ✅ Documented |
| `Admin/AuditLogsController` | `/api/admin/audit-logs` | 1 | ✅ Documented |
| `Admin/SystemHealthController` | `/api/admin/system-health` | 1 | ✅ Documented |
| `Admin/DeadLetterQueueController` | `/api/admin/dead-letter-queue` | 3 | ✅ Documented |
| `Admin/ReadModelsController` | `/api/admin/read-models` | 2 | ✅ Documented |
| `Admin/AIGovernanceController` | `/api/admin/ai` | 5 | ✅ Documented |
| `Admin/AdminMemberPermissionsController` | `/api/admin/permissions` | 2 | ✅ Documented |
| `SuperAdmin/SuperAdminAIQuotaController` | `/api/v1/superadmin/organizations` | 3 | ✅ Documented |
| `SuperAdmin/SuperAdminPermissionPolicyController` | `/api/v1/superadmin/organizations` | 2 | ✅ Documented |
| `MilestonesController` | `/api/v1/Milestones` | 9 | ✅ Documented | Includes CRUD, complete, statistics, overdue |
| `ReleasesController` | `/api/v1/Releases` | 17 | ✅ Documented |
| `TestController` | `/api/v1/Test` | 1 | ⚠️ DEBUG-only (#if DEBUG) |

#### 21.1.2 Endpoint Coverage

**Total Endpoints:** ~175 endpoints

| Category | Documented | Implemented | Coverage |
|----------|------------|-------------|----------|
| **Authentication** | 5 | 5 | 100% |
| **Projects** | 11 | 11 | 100% |
| **Tasks** | 8 | 8 | 100% |
| **Sprints** | 7 | 7 | 100% |
| **Teams** | 5 | 5 | 100% |
| **Defects** | 5 | 5 | 100% |
| **Users** | 6 | 6 | 100% |
| **Comments** | 4 | 4 | 100% |
| **Attachments** | 4 | 4 | 100% |
| **Notifications** | 4 | 4 | 100% |
| **Metrics** | 4 | 4 | 100% |
| **AI Agents** | 10 | 10 | 100% |
| **Admin** | 20 | 20 | 100% |
| **Settings** | 3 | 3 | 100% |
| **Permissions** | 2 | 2 | 100% |
| **Backlog** | 4 | 4 | 100% |
| **Alerts** | 2 | 2 | 100% |
| **Insights** | 1 | 1 | 100% |
| **Activity** | 1 | 1 | 100% |
| **Health** | 2 | 2 | 100% |
| **Feature Flags** | 6 | 6 | 100% |
| **Read Models** | 5 | 5 | 100% |
| **AI Governance** | 8 | 8 | 100% |
| **Search** | 1 | 1 | 100% |
| **Milestones** | 8 | 8 | 100% |
| **Releases** | 17 | 17 | 100% |

**Overall Coverage:** 100% (all endpoints implemented and documented)

#### 21.1.3 Endpoints Recently Added (v2.6-v2.7)

**Comments API (v2.6):**
- ✅ `GET /api/v1/Comments?entityType={type}&entityId={id}` - Query parameters (corrected from path params)
- ✅ `POST /api/v1/Comments` - Add comment
- ✅ `PUT /api/v1/Comments/{id}` - Update comment
- ✅ `DELETE /api/v1/Comments/{id}` - Delete comment

**Attachments API (v2.6):**
- ✅ `GET /api/v1/Attachments?entityType={type}&entityId={id}` - Query parameters (corrected from path params)
- ✅ `POST /api/v1/Attachments/upload` - Upload attachment (multipart/form-data)
- ✅ `GET /api/v1/Attachments/{id}` - Download attachment (no `/download` suffix)
- ✅ `DELETE /api/v1/Attachments/{id}` - Delete attachment

**Teams API (v2.7):**
- ✅ `GET /api/v1/Teams` - Get all teams (with `[RequirePermission("teams.view")]`)
- ✅ `GET /api/v1/Teams/{id}` - Get team by ID
- ✅ `POST /api/v1/Teams` - Register team (with `[RequirePermission("teams.create")]`)
- ✅ `PATCH /api/v1/Teams/{id}/capacity` - Update capacity (with `[RequirePermission("teams.edit")]`)
- ✅ `GET /api/v1/Teams/{id}/availability` - Get availability (with `[RequirePermission("teams.view.availability")]`)

#### 21.1.4 Endpoint Corrections Applied

**Comments API:**
- ✅ Changed from path parameters `/Comments/{entityType}/{entityId}` to query parameters `/Comments?entityType={type}&entityId={id}`

**Attachments API:**
- ✅ Changed from path parameters `/Attachments/{entityType}/{entityId}` to query parameters `/Attachments?entityType={type}&entityId={id}`
- ✅ Removed `/download` suffix: `/Attachments/{id}/download` → `/Attachments/{id}`

**Notifications API:**
- ✅ Changed `markAllAsRead()` from `POST` to `PATCH /api/v1/Notifications/mark-all-read`
- ✅ `GET /api/v1/Notifications/unread-count` - Implemented and documented

**Projects API:**
- ✅ Removed explicit `/api/v1` prefix in `assignTeam()` - uses relative path

**Auth API:**
- ✅ `POST /api/v1/Auth/logout` - Exists and documented
- ✅ `POST /api/v1/Auth/register` - Deprecated (returns 403)

### 21.2 Swagger Documentation Status

**Swagger Coverage:** ~95%

**Missing Documentation:**
- Some endpoints lack XML comments
- Some endpoints lack `ProducesResponseType` attributes
- Some complex response types not fully documented

**Improvements Needed:**
- Add XML documentation to all endpoints
- Add `ProducesResponseType` for all response codes
- Add examples for request/response bodies
- Document error response formats consistently

---

## Changelog

### Version 2.14.4 (January 6, 2025)
- ✅ **Admin Organization Members Tenant Isolation Fix**: Fixed tenant isolation for Admin organization members page
  - Updated `AdminOrganizationMembers.tsx` to use `/api/v1/Users` endpoint instead of `/api/admin/organization/members`
  - `/api/v1/Users` uses `OrganizationScopingService` which properly handles tenant isolation:
    - **Admin**: Automatically filters to show only members of their organization
    - **SuperAdmin**: Shows all members from all organizations
  - Fixed `usersApi.getAllPaginated()` to map frontend sortField values to backend expected values
    - Added `mapSortFieldToBackend()` function to convert frontend values (`name`, `email`, `role`, `createdAt`, `status`) to backend values (`CreatedAt`, `Email`, `Role`, `IsActive`)
    - Fixed 400 Bad Request errors when sorting by `name` (now maps to `CreatedAt`)
  - Updated `AdminOrganizationMembers.tsx` to use `UserListDto` type from `@/api/users` instead of local interface
  - Changed `user.role` to `user.globalRole` to match API response structure
  - Admin users now correctly see only their organization's members
  - SuperAdmin users correctly see all members from all organizations

### Version 2.14.3 (January 5, 2025)
- ✅ **401 Error Handling Improvements**: Enhanced error handling for expired tokens
  - Updated `NotificationBell` component to conditionally disable `refetchOnWindowFocus` based on authentication status
  - Improved retry logic to properly detect 401 errors (checks for both "Unauthorized" and "401" in error messages)
  - Added `refetchOnWindowFocus: isAuthenticated && !isAuthLoading` to prevent refetch attempts when user is not authenticated
  - Fixed 401 errors on `/api/v1/Notifications` and `/api/v1/Notifications/unread-count` endpoints during window focus refetch
- ✅ **Feature Flags Service**: Improved 401 error handling
  - Updated `featureFlagService` to re-throw 401 errors instead of catching them silently
  - Allows API client to handle token refresh automatically
  - Updated `FeatureFlagsContext` to not set error state for 401 errors (API client handles authentication)
- ✅ **Dialog Accessibility Fix**: Fixed missing DialogDescription warning
  - Added `DialogDescription` component to `AdminAIQuota` dialog
  - Resolves Radix UI accessibility warning for missing description
  - All dialogs now properly include descriptions for screen readers

### Version 2.14.2 (January 5, 2025)
- ✅ **SuperAdmin Route Fix**: Fixed SuperAdmin API routes to use proper versioning
  - Updated `SuperAdminAIQuotaController` route: `api/superadmin/organizations` → `api/v{version:apiVersion}/superadmin/organizations`
  - Updated `SuperAdminPermissionPolicyController` route: `api/superadmin/organizations` → `api/v{version:apiVersion}/superadmin/organizations`
  - Updated frontend API client to properly transform `/api/superadmin/...` to `/api/v1/superadmin/...`
  - All SuperAdmin endpoints now correctly use versioned routes: `/api/v1/superadmin/organizations/...`
  - Fixed 404 errors on SuperAdmin endpoints (`/api/v1/superadmin/organizations/{orgId}/ai-quota`, `/api/v1/superadmin/organizations/{orgId}/permission-policy`)
- ✅ **Documentation Update**: Added SuperAdmin AI Quota endpoints documentation
  - Documented `GET /api/v1/superadmin/organizations/{orgId}/ai-quota`
  - Documented `PUT /api/v1/superadmin/organizations/{orgId}/ai-quota`
  - Documented `GET /api/v1/superadmin/organizations/ai-quotas`
  - Updated controller count: 41 → 42 controllers (26 standard + 14 admin + 2 superadmin + 1 DEBUG-only TestController)
  - Updated SuperAdmin controller section to include both controllers

### Version 2.14.1 (January 4, 2025)
- ✅ **Documentation Update**: Comprehensive codebase scan and documentation refresh
  - Updated controller count: 38 → 41 controllers (26 standard + 14 admin + 1 superadmin + 1 DEBUG-only TestController)
  - Updated command count: 90 → 98 commands (accurate count from codebase scan)
  - Updated query count: 70 → 76 queries (accurate count from codebase scan)
  - Updated entity count: 39 → 44 entities (added OrganizationPermissionPolicy, UserAIQuota, OrganizationAIQuota, UserAIUsageCounter, UserAIQuotaOverride)
  - Updated endpoint count: ~171 → ~175 endpoints
  - Added missing admin controllers to documentation (AdminAIQuotaController, OrganizationsController, OrganizationController, SuperAdminAIQuotaController)
  - Verified all controllers, commands, queries, and entities are accurately documented
  - Updated "Last Updated" date to January 4, 2025

### Version 2.13.0 (January 2, 2025)
- ✅ **Billing System Removal**: Removed all billing/subscription/plan features
  - Deleted `BillingService` and `IBillingService` interfaces
  - Removed billing service registration from DependencyInjection
  - Removed billing integration from `UpdateAIQuotaCommandHandler`
  - Removed `WasBillingTriggered` and `BillingReferenceId` from `UpdateAIQuotaResponse`
  - Removed `upgradeUrl` from error handling in `Program.cs` and `client.ts`
  - Updated `AIQuota` entity comments (BillingReferenceId kept for DB compatibility only)
  - Updated all frontend components to remove billing navigation and upgrade buttons
  - Removed "Plan Comparison" section from QuotaDetails page
  - Updated error messages to direct users to contact administrators instead of upgrade prompts
  - No regressions: AI quota functionality remains fully intact

### Version 2.12.0 (January 2, 2025)
- ✅ **TestController Production Safety**: Conditioned TestController with #if DEBUG
  - Wrapped entire TestController class with `#if DEBUG` / `#endif` directives
  - Controller is now only available in Debug builds, automatically excluded from Release builds
  - Updated documentation to reflect DEBUG-only status
  - Prevents test endpoints from being accessible in production
- ✅ **Code Cleanup**: Removed test code from production paths
  - TestController excluded from Release builds via compilation directives
  - Maintains development utility while ensuring production safety
- ✅ **Documentation Updates**:
  - Updated controller count (37 total, TestController is conditional)
  - Added note about TestController being DEBUG-only
  - Updated API endpoints audit section

### Version 2.9.3 (December 31, 2024)
- ✅ **HealthApiController**: API smoke tests endpoint for monitoring
  - Created `HealthApiController` with endpoint `GET /api/health/api` (no versioning)
  - Performs smoke tests on critical API endpoints (Auth, Projects, Health, Swagger)
  - Tests routing and authentication with timeout protection (5 seconds per request)
  - Returns detailed check results with response times and status codes
  - Used by Docker health checks for container monitoring
  - Comprehensive test suite in `HealthApiEndpoint_Tests.cs`
- ✅ **Docker Health Check Update**: Updated docker-compose.yml health check
  - Changed health check endpoint from `/api/health/live` to `/api/health/api`
  - Uses API smoke tests for more comprehensive container health validation
- ✅ **Documentation Updates**:
  - Added HealthApiController to controllers list (36 total controllers)
  - Updated API endpoints audit with HealthApiController endpoint
  - Added comprehensive health checks documentation in `docs/HEALTH_CHECKS.md`
  - Updated controller count and endpoint coverage statistics

### Version 2.9.2 (December 30, 2024)
- ✅ **ReleasesController Unit Tests**: Comprehensive test suite for ReleasesController
  - Created `ReleasesControllerTests.cs` with 69 unit tests
  - Tests cover all 17 endpoints with multiple scenarios (happy path, error cases, edge cases)
  - Uses xUnit, Moq, and FluentAssertions following AAA pattern
  - Test coverage includes: success cases (200, 201, 204), authorization failures (403), not found errors (404), validation errors (400)
  - All 69 tests passing with 100% pass rate
  - Fixed namespace conflict with `Unit` directory by using fully qualified `MediatR.Unit.Value`
- ✅ **API Connectivity Testing**: Frontend test utility for API endpoint verification
  - Created `testReleaseApiConnectivity.ts` utility for testing all 17 Release API endpoints
  - Supports read-only and mutation test modes
  - Color-coded console output with detailed error reporting
  - Available globally via `testReleaseApi()` function in browser console
- ✅ **Documentation Updates**:
  - Updated test projects section to include ReleasesControllerTests
  - Added API connectivity testing documentation

### Version 2.9.1 (December 30, 2024)
- ✅ **ReleasesController Implementation**: Complete implementation with 17 endpoints
  - Added `ReleasesController` to controllers list (37 total controllers)
  - Implemented all 17 endpoints with proper `[RequirePermission]` attributes
  - Added comprehensive API documentation for all release endpoints (Section 14.14)
  - Updated endpoint coverage table to include Releases (17 endpoints, 100% coverage)
  - Updated controller structure documentation
- ✅ **Documentation Updates**:
  - Added Releases API reference section (14.14) with all 17 endpoints documented
  - Updated controller count from 36 to 37
  - Added ReleasesController to API Layer structure
  - Updated Releases feature section with detailed implementation status

### Version 2.9 (December 29, 2024)
- ✅ **Milestones Feature**: Complete milestone management system
  - Created `Milestone` entity with `MilestoneStatus` and `MilestoneType` enums
  - Implemented `MilestonesController` with full CRUD operations
  - Added milestone statistics and next milestone queries
  - Created domain events: `MilestoneCreatedEvent`, `MilestoneCompletedEvent`, `MilestoneMissedEvent`
  - Frontend components for milestone management
- ✅ **Releases Feature**: Complete release management system
  - Created `Release` entity with `ReleaseStatus` and `ReleaseType` enums
  - Created `QualityGate` entity for quality checks
  - Implemented release notes generation
  - Implemented quality gates checking
  - Release planning with sprint linking via `Sprint.ReleaseId`
  - Created domain events: `ReleaseNotesGeneratedEvent`, `QualityGatesEvaluatedEvent`
  - Frontend components for release management
- ✅ **Task Dependencies**: Partial implementation
  - Created `TaskDependency` entity
  - Frontend components and API client ready
  - Backend endpoints need verification/implementation
- ✅ **Documentation Updates**: 
  - Updated entity count (39 entities)
  - Updated domain events count (23 events)
  - Updated controller count (36 controllers)
  - Added Milestones and Releases to implemented features
  - Updated missing features section

### Version 2.8 (December 19, 2024)
- ✅ **API Audit**: Comprehensive audit of all endpoints vs documentation
- ✅ **Missing Features Documentation**: Added section documenting missing entities (TaskDependency, Milestone, Release, TestCase, TestExecution)
- ✅ **API Endpoints Audit**: Complete comparison of real vs documented endpoints
- ✅ **Teams API Permissions**: Added `[RequirePermission]` attributes to all Teams endpoints
- ✅ **Teams Multi-Tenancy**: Fixed `RegisterTeamCommandHandler` to set `OrganizationId` correctly
- ✅ **Permissions Seeding**: Updated `DataSeeder` to add missing permissions incrementally
- ✅ **RBAC Implementation**: Complete permission matrix with 17 categories and 5 roles
- ✅ **Multi-Organization Seeder**: Added `MultiOrgDataSeeder` for testing multi-tenancy
- ✅ **Documentation Updates**: 
  - Updated endpoint coverage statistics
  - Documented endpoint corrections applied
  - Added missing features roadmap
  - Updated API reference with new endpoints

### Version 2.7 (December 26, 2024)
- ✅ **Project Deletion**: Enhanced permanent project deletion handler
  - Added comprehensive deletion of all related entities (Comments, Attachments, ProjectTeams, Notifications)
  - Improved null-safety checks for navigation properties
  - Fixed 500 errors during project deletion
  - Proper cascade deletion order to respect foreign key constraints
- ✅ **Project Endpoints**: Added new project management endpoints
  - `GET /api/v1/Projects/{id}/my-role`: Get current user's role in a project
  - `DELETE /api/v1/Projects/{id}/permanent`: Permanently delete project and all related data
- ✅ **Authentication Improvements**: Enhanced authentication error handling
  - Improved error handling in `AuthController.GetMe` endpoint
  - Added automatic token refresh on 401 errors in frontend
  - Enhanced logging for cookie-based authentication debugging
  - Better error messages for expired/invalid tokens
- ✅ **Rate Limiting**: Increased authentication rate limit
  - Auth rate limit increased from 5 to 30 requests/minute
  - Prevents 429 errors during development and testing
- ✅ **Error Handling**: Improved error handling across controllers
  - Added `ValidationException` handling in `ProjectsController` for member management
  - Better error messages for 400 Bad Request responses
  - Enhanced frontend error display with detailed backend messages
- ✅ **Documentation**: Comprehensive backend codebase scan and documentation update
  - Updated entity count (35 entities)
  - Updated controller count (32 active controllers, 8 admin controllers)
  - Added all 18 domain events to documentation
  - Added cookie-based authentication details
  - Updated API reference with new endpoints
  - Added comprehensive domain event documentation

### Version 2.7 (January 1, 2025)
- ✅ **Exception Handling**: Enhanced exception handling in ProjectsController member endpoints
  - Added `UnauthorizedAccessException` catch blocks to `GetProjectMembers`, `InviteMember`, `ChangeMemberRole`, and `RemoveMember` methods
  - All unauthorized access exceptions now correctly return `403 Forbidden` status codes
  - Added `ValidationException` catch blocks to member management endpoints for better error messages
  - Improved error handling consistency across all project member operations
- ✅ **JSON Serialization**: Configured enum serialization as strings
  - Added `JsonStringEnumConverter` to `AddJsonOptions` in `Program.cs`
  - Enums (e.g., `GlobalRole`, `ProjectRole`) now serialize as strings (e.g., `"Admin"`) instead of numbers
  - Improves API readability and frontend integration
- ✅ **Test Infrastructure**: Improved test infrastructure for integration tests
  - Updated `CustomWebApplicationFactory` to set JWT environment variables before configuration loads
  - Added `UseContentRoot(Path.GetTempPath())` for test environments to prevent file system permission errors
  - Added helper methods `EnsureOrganizationExistsAsync` and `SeedProjectMemberPermissionsAsync` for test setup
  - Tests now properly handle organization and permission prerequisites
- ✅ **Environment Configuration**: Enhanced environment handling
  - Updated `Program.cs` to treat "Testing" environment similar to "Development" for content root path configuration
  - Improved health checks database path resolution for test environments
- ✅ **Documentation**: Updated API reference and configuration documentation
  - Documented exception handling behavior for project member endpoints
  - Added JSON serialization configuration details
  - Updated testing section with test infrastructure improvements

### Version 2.6 (December 25, 2024)
- ✅ **Comment System**: Complete comment system with threading support
  - Comment entity with polymorphic relationships (Task, Project, Sprint, Defect, BacklogItem)
  - AddCommentCommand with mention parsing
  - CommentAddedEvent, CommentUpdatedEvent, CommentDeletedEvent domain events
  - CommentSection, CommentForm, CommentItem frontend components
- ✅ **Mention System**: User mention tracking and notifications
  - Mention entity with position tracking (StartIndex, Length)
  - MentionParser service for parsing @username mentions from content
  - UserMentionedEvent domain event for mention notifications
  - Automatic notification creation for mentioned users
- ✅ **Notification Preferences**: User-configurable notification settings
  - NotificationPreference entity per user and notification type
  - Email, in-app, and push channel preferences
  - Frequency settings (instant, daily, weekly, never)
  - NotificationPreferenceService for checking preferences
  - Default preferences initialization for new users
- ✅ **File Attachments**: File upload and download system
  - Attachment entity with polymorphic relationships
  - LocalFileStorageService for file storage operations
  - File validation (size limits, allowed types, extensions)
  - Attachment upload/download endpoints
  - AttachmentUpload and AttachmentList frontend components
- ✅ **AI Governance**: Complete AI decision logging and quota management
  - AIDecisionLog entity for decision tracking and explainability
  - AIQuota entity for usage tracking per organization
  - AIGovernanceController (user) and AdminAIGovernanceController (admin)
  - Quota management commands (UpdateAIQuotaCommand, DisableAIForOrgCommand, EnableAIForOrgCommand)
  - Decision approval/rejection commands
  - AIAvailabilityService for checking AI availability
  - AIGovernance frontend page with overview, decisions, and quotas tabs
- ✅ **Read Models**: CQRS read models for optimized queries
  - TaskBoardReadModel, SprintSummaryReadModel, ProjectOverviewReadModel entities
  - ReadModelsController for accessing read model data
  - Admin endpoint for rebuilding read models
- ✅ **Services**: New application services
  - IMentionParser: Parses @username mentions from text
  - INotificationPreferenceService: Manages notification preferences
  - IFileStorageService: Handles file storage operations
  - IAIAvailabilityService: Checks AI availability for organizations
- ✅ **Domain Events**: New domain events for comment system
  - CommentAddedEvent: Published when comment is added
  - CommentUpdatedEvent: Published when comment is updated
  - CommentDeletedEvent: Published when comment is deleted
  - UserMentionedEvent: Published when user is mentioned
- ✅ **Constants**: New domain constants
  - NotificationConstants: Notification types and frequencies
  - AttachmentConstants: File size limits and allowed types
  - AIDecisionConstants: AI decision types and statuses
  - AIQuotaConstants: AI quota tiers and limits
- 📝 **Documentation**: Updated API reference with new endpoints and improved documentation

### Version 2.5 (December 25, 2024)
- ✅ **User Entity**: Added `LastLoginAt` field to track user login timestamps
- ✅ **Authentication**: Updated `AuthService.LoginAsync` to set `LastLoginAt` on successful login
- ✅ **User Management**: Added `LastLoginAt` to `UserListDto` and admin user listing
- ✅ **Database Migration**: Created migration `AddLastLoginAtToUser` for schema update
- ✅ **Settings Management**: Complete implementation of Global Settings with categories (General, Security, Email, FeatureFlags)
- ✅ **Email Settings**: Full SMTP configuration support (host, port, username, password, SSL/TLS, from email/name)
- ✅ **Security Settings**: Password policy configuration (min length, uppercase, lowercase, digits, special chars, max login attempts, session duration, 2FA requirement)
- ✅ **General Settings**: Application name, default timezone, default language, date format configuration
- ✅ **Test Email**: Added `POST /api/v1/Settings/test-email` endpoint for SMTP testing
- ✅ **User Detail Endpoints**: Added `GET /api/v1/Users/{id}/projects` and `GET /api/v1/Users/{id}/activity` endpoints
- ✅ **Admin Dashboard**: Fixed 500 error in `GetAdminDashboardStatsQueryHandler` with improved error handling and query optimization
- ✅ **User Invitation**: Improved error handling in `InviteOrganizationUserCommandHandler` - invitation created even if email fails
- ✅ **Swagger Configuration**: Enhanced Swagger generation to handle complex types (Dictionary<string, ExternalServiceStatus>)
- ✅ **CurrentUserService**: Added `AsNoTracking()` to improve performance and prevent tracking issues
- ✅ **Dead Letter Queue**: Implemented DLQ functionality for failed Outbox messages
  - Created `DeadLetterMessage` entity for tracking failed messages
  - Added `DeadLetterQueueController` with GET, POST (retry), and DELETE endpoints
  - Updated `OutboxProcessor` to move messages to DLQ after max retries
  - Added DLQ count to system health dashboard
- ✅ **ProjectTeam Entity**: Created explicit many-to-many relationship between Project and Team
  - Added `ProjectTeam` entity with assignment tracking
  - Created EF Core migration for ProjectTeams table
  - Updated `Project` and `Team` entities with navigation properties
- ✅ **AssignTeamToProject Command**: Implemented team-to-project assignment
  - Created `AssignTeamToProjectCommand` with default role and member role overrides
  - Added `POST /api/v1/Projects/{projectId}/assign-team` endpoint
  - Automatically creates `ProjectTeam` entry and adds team members as `ProjectMembers`
- ✅ **UserCreatedEvent & UserUpdatedEvent**: Implemented domain events with automatic auditing
  - Created `UserCreatedEvent` and `UserUpdatedEvent` domain events
  - Integrated with Outbox pattern for reliable event publishing
  - Created event handlers to automatically generate audit logs
  - Added idempotency keys to prevent duplicate processing
- ✅ **ActivateUserCommand & DeactivateUserCommand**: Created dedicated commands for user activation/deactivation
  - Replaced bulk status update pattern with explicit commands
  - Added `POST /api/admin/users/{userId}/activate` and `/deactivate` endpoints
  - Prevents self-deactivation in `DeactivateUserCommand`
  - Invalidates refresh tokens on deactivation
- 📝 **Documentation**: Updated API reference with new endpoints and improved error handling documentation

### Version 2.4 (December 24, 2024)
- ✅ **Documentation**: Comprehensive codebase analysis and documentation update
- ✅ **Controllers**: Updated controller count (28 active controllers)
- ✅ **Entities**: Updated entity count (24 entities)
- ✅ **Admin Controllers**: Documented all 5 admin controllers (Users, FeatureFlags, Dashboard, AuditLogs, SystemHealth)
- ✅ **API Endpoints**: Complete list of all controllers and their purposes
- ✅ **Structure**: Accurate file counts and organization documented

### Version 2.3 (December 24, 2024)
- ✅ **Feature Flags Controller**: Fixed route to use explicit kebab-case path (`/api/v1/feature-flags`)
- ✅ **Feature Flags Query**: Added `GetFeatureFlagByNameQuery` for retrieving individual flags by name
- ✅ **Feature Flags Endpoint**: Added `GET /api/v1/feature-flags/{name}` endpoint for single flag retrieval
- ✅ **API Versioning**: Documented admin routes pattern (no versioning in URL) vs standard routes (with versioning)
- 📝 **Documentation**: Updated API reference to include public feature flags endpoints vs admin endpoints

### Version 2.2 (December 23, 2024)
- ✅ **Organization Invitations**: Added organization-level user invitation flow
- ✅ **Password Reset**: Added password reset functionality
- ✅ **Email Templates**: Added organization invitation email template
- ✅ **Admin User Management**: Added invite user endpoint

---

## Appendix A: Entity Relationship Diagram

```
Organization
    ├── Users (1:N)
    └── Projects (1:N)
        ├── ProjectMembers (1:N)
        ├── ProjectTeams (M:N via ProjectTeam)
        ├── Sprints (1:N)
        ├── ProjectTasks (1:N)
        ├── Defects (1:N)
        ├── BacklogItems (1:N)
Teams (1:N)
    └── ProjectTeams (M:N via ProjectTeam)
```

---

## Appendix B: Permission Matrix

| Permission | Admin | User |
|------------|-------|------|
| `users.create` | ✅ | ❌ |
| `users.update` | ✅ | ❌ |
| `users.delete` | ✅ | ❌ |
| `admin.settings.update` | ✅ | ❌ |
| `admin.permissions.update` | ✅ | ❌ |
| `projects.create` | ✅ | ✅* |
| `projects.view` | ✅ | ✅ |

*Depends on `ProjectCreation.AllowedRoles` setting

---

## Appendix C: Project Role Permissions

| Permission | ProductOwner | ScrumMaster | Developer | Tester | Viewer |
|------------|--------------|-------------|-----------|--------|--------|
| Edit Project | ✅ | ✅ | ❌ | ❌ | ❌ |
| Delete Project | ✅ | ❌ | ❌ | ❌ | ❌ |
| Invite Members | ✅ | ✅ | ❌ | ❌ | ❌ |
| Remove Members | ✅ | ✅ | ❌ | ❌ | ❌ |
| Change Roles | ✅ | ❌ | ❌ | ❌ | ❌ |
| Create Tasks | ✅ | ✅ | ✅ | ✅ | ❌ |
| Edit Tasks | ✅ | ✅ | ✅ | ✅ | ❌ |
| Delete Tasks | ✅ | ✅ | ❌ | ❌ | ❌ |
| Manage Sprints | ✅ | ✅ | ❌ | ❌ | ❌ |

---

### Version 2.14.0 (January 2, 2025)
- ✅ **Organization Permission Policy System**: Complete permission policy management with two-level enforcement
  - **New Entity**: `OrganizationPermissionPolicy` - Defines allowed permissions per organization
    - Unique constraint: one policy per organization
    - JSON storage for permission names array
    - `IsActive` flag to enable/disable policy (inactive = allow all)
    - Default behavior: if no policy exists, all permissions allowed
  - **New Service**: `OrganizationPermissionPolicyService` - Permission policy enforcement service
    - `GetPolicyAsync()`: Retrieves organization policy (returns null if not exists)
    - `IsPermissionAllowedAsync()`: Checks if permission is allowed
    - `ValidatePermissionsAsync()`: Validates multiple permissions (throws if any disallowed)
    - `GetAllowedPermissionsAsync()`: Gets all allowed permissions
  - **SuperAdmin Endpoints**: `/api/superadmin/organizations/{orgId}/permission-policy`
    - `GET`: Get organization permission policy (returns default if not exists)
    - `PUT`: Upsert permission policy (create or update)
    - Validates all permission names exist in system
    - Returns `400 Bad Request` if invalid permissions specified
  - **Admin Endpoints**: `/api/admin/permissions/members`
    - `GET`: Get paginated list of organization members with permissions (own organization only)
    - `PUT /members/{userId}`: Update member role and/or permissions
    - Policy enforcement: assigned permissions must be subset of org allowed permissions
    - Tenant isolation: Admin can only manage members in their own organization
    - Self-modification prevention: Admin cannot change own role/permissions
    - Role restrictions: Admin cannot assign SuperAdmin role
  - **New Commands/Queries**:
    - `UpsertOrganizationPermissionPolicyCommand` + Handler + Validator
    - `GetOrganizationPermissionPolicyQuery` + Handler
    - `UpdateMemberPermissionCommand` + Handler + Validator
    - `GetMemberPermissionsQuery` + Handler
  - **New DTOs**:
    - `OrganizationPermissionPolicyDto`: Policy information with organization details
    - `UpdateOrganizationPermissionPolicyRequest`: Request DTO for policy updates
    - `MemberPermissionDto`: Member information with permissions
    - `UpdateMemberPermissionRequest`: Request DTO for member permission updates
  - **Integration Points**:
    - `UpdateRolePermissionsCommandHandler`: Enforces organization policy when Admin updates role permissions
    - `UpdateMemberPermissionCommandHandler`: Enforces organization policy when Admin updates member permissions
    - `OrganizationScopingService`: Ensures tenant isolation for Admin operations
  - **Database Migration**: `AddOrganizationPermissionPolicy` migration
    - Creates `OrganizationPermissionPolicies` table
    - Unique index on `OrganizationId`
    - Foreign key to `Organizations` table with `DeleteBehavior.Restrict`
  - **Controller Count**: Updated from 38 to 41 controllers (added AdminMemberPermissionsController, SuperAdminPermissionPolicyController, AdminAIQuotaController, OrganizationsController, OrganizationController, SuperAdminAIQuotaController)
  - **Command/Query Count**: Updated from 90/70 to 98/76 (added 8 commands, 6 queries including permission policy and AI quota management)
  - **Entity Count**: Updated from 40 to 44 entities (added OrganizationPermissionPolicy, UserAIQuota, OrganizationAIQuota, UserAIUsageCounter, UserAIQuotaOverride)
  - **Documentation Update** (January 4, 2025):
    - Updated controller count from 38 to 41 (accurate count: 26 standard + 14 admin + 1 superadmin + 1 DEBUG-only TestController)
    - Updated command count from 90 to 98 (accurate count from codebase scan)
    - Updated query count from 70 to 76 (accurate count from codebase scan)
    - Updated endpoint count from ~171 to ~175 endpoints
    - Added missing admin controllers to controller list (AdminAIQuotaController, OrganizationsController, OrganizationController, SuperAdminAIQuotaController)

### Version 2.11.0 (January 1, 2025)
- ✅ **Statistics Update**: Updated documentation with actual implementation counts from comprehensive audit
  - 36 controllers (27 standard + 8 admin + 1 DEBUG-only TestController)
  - ~171 endpoints (was ~120+)
  - 88 Commands (CQRS) - all with Handlers
  - 68 Queries (CQRS) - all with Handlers
  - 44 entities - all configured (17 dedicated + 27 inline)
- ✅ **Validators**: Most Commands have FluentValidation validators
  - ⚠️ **Note**: A few Commands (DeleteTaskCommand, UpdateSprintCommand) have inline validation instead of dedicated validators
  - ✅ **Recommendation**: Create dedicated validators for consistency
- ✅ **Swagger Documentation**: Improved documentation for Projects, Tasks, and Agents controllers
  - Added XML comments with examples
  - Added ProducesResponseType attributes
  - ⚠️ **Note**: Some endpoints still need complete Swagger documentation
- ✅ **Permissions**: All endpoints have `[RequirePermission]` attributes
- ✅ **EF Core Configurations**: All 44 entities have configurations
  - 17 entities have dedicated configuration files
  - 27 entities configured inline in AppDbContext.cs
  - All relations properly configured with DeleteBehavior.Restrict
- ✅ **CQRS Coverage**: Complete CQRS implementation
  - All Commands have Handlers
  - All Queries have Handlers
  - Most Commands have Validators (95% coverage)

### Version 2.10.0 (January 1, 2025)
- ✅ **Sprint Retrospective Generation**: New AI agent endpoint for generating sprint retrospectives
  - Added `POST /api/v1/Agent/generate-retrospective/{sprintId}` endpoint
  - Created SprintRetrospectivePlugin with Semantic Kernel functions
  - Plugin functions: GetSprintMetrics, GetCompletedTasks, GetIncompleteTasks, GetSprintDefects, GetTeamActivity
  - Validates sprint is completed before generating retrospective
  - Returns structured retrospective data with metrics, tasks, defects, and team activity
  - Saves retrospective notes to Sprint.RetrospectiveNotes
- ✅ **Task Improvement Endpoint**: Enhanced task improvement with better validation
  - Improved validation (max 5000 characters, description required)
  - Better error handling with specific status codes (400, 401, 500)
  - Enhanced logging with user context
- ✅ **AI Governance Endpoints**: Complete AI governance API documented
  - User endpoints: Get decisions, get quota status, get usage statistics
  - Admin endpoints: Manage quotas, disable/enable AI, export decisions, overview stats
  - Comprehensive error handling and validation
- ✅ **Documentation Updates**:
  - Updated API endpoint reference (Agent endpoints section needs update)
  - Added SprintRetrospectivePlugin documentation
  - Updated changelog with all new features

### Version 2.8 (December 19, 2024)
- ✅ **API Audit**: Comprehensive audit of all endpoints vs documentation
- ✅ **Missing Features Documentation**: Added section documenting missing entities (TaskDependency, Milestone, Release, TestCase, TestExecution)
- ✅ **API Endpoints Audit**: Complete comparison of real vs documented endpoints
- ✅ **Teams API Permissions**: Added `[RequirePermission]` attributes to all Teams endpoints
- ✅ **Teams Multi-Tenancy**: Fixed `RegisterTeamCommandHandler` to set `OrganizationId` correctly
- ✅ **Permissions Seeding**: Updated `DataSeeder` to add missing permissions incrementally
- ✅ **RBAC Implementation**: Complete permission matrix with 17 categories and 5 roles
- ✅ **Multi-Organization Seeder**: Added `MultiOrgDataSeeder` for testing multi-tenancy
- ✅ **Documentation Updates**: 
  - Updated endpoint coverage statistics
  - Documented endpoint corrections applied
  - Added missing features roadmap
  - Updated API reference with new endpoints

### Version 2.7 (December 26, 2024)

