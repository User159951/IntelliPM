using Microsoft.EntityFrameworkCore;
using IntelliPM.Domain.Entities;
using IntelliPM.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using IntelliPM.Infrastructure.Persistence.Configurations;

namespace IntelliPM.Infrastructure.Persistence;

/// <summary>
/// SQL Server DbContext for transactional data (Users, Projects, Backlogs, Sprints, etc.)
/// </summary>
public class AppDbContext : DbContext
{
    private readonly IServiceProvider? _serviceProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public AppDbContext(DbContextOptions<AppDbContext> options, IServiceProvider serviceProvider) : base(options)
    {
        _serviceProvider = serviceProvider;
    }

    public DbSet<Organization> Organizations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<Epic> Epics { get; set; }
    public DbSet<Feature> Features { get; set; }
    public DbSet<UserStory> UserStories { get; set; }
    public DbSet<Domain.Entities.Task> Tasks { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
    public DbSet<Sprint> Sprints { get; set; }
    public DbSet<SprintItem> SprintItems { get; set; }
    public DbSet<KPISnapshot> KPISnapshots { get; set; }
    public DbSet<Risk> Risks { get; set; }
    public DbSet<DocumentStore> DocumentStores { get; set; }
    public DbSet<AIAgentRun> AIAgentRuns { get; set; }
    public DbSet<AIDecision> AIDecisions { get; set; }
    public DbSet<AgentExecutionLog> AgentExecutionLogs { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<Defect> Defects { get; set; }
    public DbSet<Insight> Insights { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Invitation> Invitations { get; set; }
    public DbSet<OrganizationInvitation> OrganizationInvitations { get; set; }
    public DbSet<GlobalSetting> GlobalSettings { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<DeadLetterMessage> DeadLetterMessages { get; set; }
    public DbSet<FeatureFlag> FeatureFlags { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ProjectTeam> ProjectTeams { get; set; }
    public DbSet<TaskBoardReadModel> TaskBoardReadModels { get; set; }
    public DbSet<SprintSummaryReadModel> SprintSummaryReadModels { get; set; }
    public DbSet<ProjectOverviewReadModel> ProjectOverviewReadModels { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Mention> Mentions { get; set; }
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<AIDecisionLog> AIDecisionLogs { get; set; }
    public DbSet<AIQuota> AIQuotas { get; set; }
    public DbSet<TaskDependency> TaskDependencies { get; set; }
    public DbSet<Milestone> Milestones { get; set; }
    public DbSet<Release> Releases { get; set; }
    public DbSet<QualityGate> QualityGates { get; set; }
        public DbSet<UserAIQuotaOverride> UserAIQuotaOverrides { get; set; }
        public DbSet<UserAIUsageCounter> UserAIUsageCounters { get; set; }
        public DbSet<OrganizationAIQuota> OrganizationAIQuotas { get; set; }
        public DbSet<UserAIQuota> UserAIQuotas { get; set; }
        public DbSet<OrganizationPermissionPolicy> OrganizationPermissionPolicies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // FIX: Disable cascade delete on all FK relationships to prevent cycles
        // SQL Server doesn't allow multiple cascade paths
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // Organization configuration is applied via OrganizationConfiguration.cs (see below)

        // GlobalSetting
        modelBuilder.Entity<GlobalSetting>()
            .ToTable("GlobalSettings");
        modelBuilder.Entity<GlobalSetting>()
            .HasKey(gs => gs.Id);
        modelBuilder.Entity<GlobalSetting>()
            .Property(gs => gs.Key)
            .IsRequired()
            .HasMaxLength(100);
        modelBuilder.Entity<GlobalSetting>()
            .Property(gs => gs.Value)
            .IsRequired()
            .HasMaxLength(1000);
        modelBuilder.Entity<GlobalSetting>()
            .Property(gs => gs.Description)
            .HasMaxLength(500);
        modelBuilder.Entity<GlobalSetting>()
            .Property(gs => gs.CreatedAt)
            .IsRequired();
        modelBuilder.Entity<GlobalSetting>()
            .HasIndex(gs => gs.Key)
            .IsUnique();
        modelBuilder.Entity<GlobalSetting>()
            .Property(gs => gs.Category)
            .HasMaxLength(50)
            .HasDefaultValue("General");
        modelBuilder.Entity<GlobalSetting>()
            .HasOne(gs => gs.UpdatedBy)
            .WithMany()
            .HasForeignKey(gs => gs.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Permission
        modelBuilder.Entity<Permission>()
            .ToTable("Permissions");
        modelBuilder.Entity<Permission>()
            .HasKey(p => p.Id);
        modelBuilder.Entity<Permission>()
            .Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);
        modelBuilder.Entity<Permission>()
            .Property(p => p.Category)
            .IsRequired()
            .HasMaxLength(100);
        modelBuilder.Entity<Permission>()
            .Property(p => p.CreatedAt)
            .IsRequired();
        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.Name)
            .IsUnique();

        // RolePermission
        modelBuilder.Entity<RolePermission>()
            .ToTable("RolePermissions");
        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => rp.Id);
        modelBuilder.Entity<RolePermission>()
            .Property(rp => rp.Role)
            .IsRequired();
        modelBuilder.Entity<RolePermission>()
            .Property(rp => rp.CreatedAt)
            .IsRequired();
        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        modelBuilder.Entity<RolePermission>()
            .HasIndex(rp => new { rp.Role, rp.PermissionId })
            .IsUnique();

        // AuditLog
        modelBuilder.Entity<AuditLog>()
            .ToTable("AuditLogs");
        modelBuilder.Entity<AuditLog>()
            .HasKey(al => al.Id);
        modelBuilder.Entity<AuditLog>()
            .Property(al => al.Action)
            .IsRequired()
            .HasMaxLength(100);
        modelBuilder.Entity<AuditLog>()
            .Property(al => al.EntityType)
            .IsRequired()
            .HasMaxLength(100);
        modelBuilder.Entity<AuditLog>()
            .Property(al => al.EntityName)
            .HasMaxLength(255);
        modelBuilder.Entity<AuditLog>()
            .Property(al => al.IpAddress)
            .HasMaxLength(45); // IPv6 max length
        modelBuilder.Entity<AuditLog>()
            .Property(al => al.UserAgent)
            .HasMaxLength(500);
        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => al.UserId);
        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => al.EntityType);
        modelBuilder.Entity<AuditLog>()
            .HasIndex(al => al.CreatedAt);
        modelBuilder.Entity<AuditLog>()
            .HasOne(al => al.User)
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>()
            .HasOne(u => u.Organization)
            .WithMany(o => o.Users)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Project
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Owner)
            .WithMany()
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Organization)
            .WithMany()
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        modelBuilder.Entity<Project>()
            .Property(p => p.RowVersion)
            .IsRowVersion();

        // ProjectMembers
        modelBuilder.Entity<ProjectMember>()
            .HasIndex(pm => new { pm.ProjectId, pm.UserId }).IsUnique();
        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.InvitedBy)
            .WithMany()
            .HasForeignKey(pm => pm.InvitedById)
            .OnDelete(DeleteBehavior.Restrict);

        // BacklogItem inheritance (TPH - Table Per Hierarchy)
        modelBuilder.Entity<BacklogItem>()
            .HasDiscriminator<string>("ItemType")
            .HasValue<Epic>("Epic")
            .HasValue<Feature>("Feature")
            .HasValue<UserStory>("Story");

        // Sprint
        modelBuilder.Entity<Sprint>()
            .HasIndex(s => new { s.ProjectId, s.Number }).IsUnique();
        modelBuilder.Entity<Sprint>()
            .HasOne(s => s.Organization)
            .WithMany()
            .HasForeignKey(s => s.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        modelBuilder.Entity<Sprint>()
            .Property(s => s.RowVersion)
            .IsRowVersion();
        // Index on ReleaseId for sprint-release relationship
        modelBuilder.Entity<Sprint>()
            .HasIndex(s => s.ReleaseId)
            .HasDatabaseName("IX_Sprints_ReleaseId");
        // Relationship: Sprint -> Release (optional, SetNull on delete)
        modelBuilder.Entity<Sprint>()
            .HasOne(s => s.Release)
            .WithMany(r => r.Sprints)
            .HasForeignKey(s => s.ReleaseId)
            .OnDelete(DeleteBehavior.SetNull);

        // SprintItem
        modelBuilder.Entity<SprintItem>()
            .HasIndex(si => new { si.SprintId, si.UserStoryId }).IsUnique();

        // Risk
        modelBuilder.Entity<Risk>()
            .HasOne(r => r.Owner)
            .WithMany()
            .HasForeignKey(r => r.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        // RefreshToken
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token).IsUnique();

        // PasswordResetToken
        modelBuilder.Entity<PasswordResetToken>()
            .HasIndex(prt => prt.Token).IsUnique();
        modelBuilder.Entity<PasswordResetToken>()
            .HasIndex(prt => prt.UserId);

        // DocumentStore
        modelBuilder.Entity<DocumentStore>()
            .HasIndex(ds => ds.ProjectId);

        // Defect
        modelBuilder.Entity<Defect>()
            .HasIndex(d => new { d.ProjectId, d.Status });
        modelBuilder.Entity<Defect>()
            .HasIndex(d => d.Severity);
        modelBuilder.Entity<Defect>()
            .HasOne(d => d.Organization)
            .WithMany()
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Insight
        modelBuilder.Entity<Insight>()
            .HasIndex(i => new { i.ProjectId, i.Status });
        modelBuilder.Entity<Insight>()
            .HasIndex(i => i.AgentType);
        modelBuilder.Entity<Insight>()
            .Property(i => i.Confidence)
            .HasColumnType("decimal(5,2)"); // Fix: Specify precision and scale

        // KPISnapshot
        modelBuilder.Entity<KPISnapshot>()
            .Property(k => k.CycleTimeDays)
            .HasColumnType("decimal(10,2)"); // Fix: Specify precision and scale
        modelBuilder.Entity<KPISnapshot>()
            .Property(k => k.LeadTimeDays)
            .HasColumnType("decimal(10,2)"); // Fix: Specify precision and scale

        // Alert
        modelBuilder.Entity<Alert>()
            .HasIndex(a => new { a.ProjectId, a.IsResolved });

        // AIAgentRun
        modelBuilder.Entity<AIAgentRun>()
            .HasIndex(a => new { a.ProjectId, a.ExecutedAt });
        modelBuilder.Entity<AIAgentRun>()
            .Property(a => a.Confidence)
            .HasColumnType("decimal(5,2)"); // Fix: Specify precision and scale

        // Configure StoryPoints as owned entity (value object)
        modelBuilder.Entity<ProjectTask>()
            .OwnsOne(t => t.StoryPoints, sp =>
            {
                sp.Property(p => p.Value).HasColumnName("StoryPoints");
            });
        modelBuilder.Entity<ProjectTask>()
            .HasOne(t => t.Organization)
            .WithMany()
            .HasForeignKey(t => t.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Configure AgentExecutionLog decimal precision and foreign key
        modelBuilder.Entity<AgentExecutionLog>()
            .Property(a => a.ExecutionCostUsd)
            .HasColumnType("decimal(10,4)");
        
        // Configure foreign key relationship to Organization (multi-tenancy)
        modelBuilder.Entity<AgentExecutionLog>()
            .HasOne(a => a.Organization)
            .WithMany()
            .HasForeignKey(a => a.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        // Configure foreign key relationship to AIDecisionLog
        modelBuilder.Entity<AgentExecutionLog>()
            .HasOne(a => a.LinkedDecision)
            .WithMany()
            .HasForeignKey(a => a.LinkedDecisionId)
            .OnDelete(DeleteBehavior.SetNull); // Set to null if decision log is deleted

        // Team
        modelBuilder.Entity<Team>()
            .HasOne(t => t.Organization)
            .WithMany()
            .HasForeignKey(t => t.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Activity - Index for performance
        modelBuilder.Entity<Activity>()
            .HasIndex(a => new { a.ProjectId, a.CreatedAt });
        modelBuilder.Entity<Activity>()
            .HasIndex(a => a.UserId);
        modelBuilder.Entity<Activity>()
            .HasIndex(a => new { a.EntityType, a.EntityId });

        // Notification - Index for performance
        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
        modelBuilder.Entity<Notification>()
            .HasIndex(n => n.UserId);
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Organization)
            .WithMany()
            .HasForeignKey(n => n.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Invitation
        modelBuilder.Entity<Invitation>()
            .HasIndex(i => i.Token).IsUnique();
        modelBuilder.Entity<Invitation>()
            .HasIndex(i => new { i.Email, i.IsUsed, i.ExpiresAt });
        modelBuilder.Entity<Invitation>()
            .HasOne(i => i.CreatedBy)
            .WithMany()
            .HasForeignKey(i => i.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Invitation>()
            .HasOne(i => i.Project)
            .WithMany()
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Invitation>()
            .HasOne(i => i.Organization)
            .WithMany()
            .HasForeignKey(i => i.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // OutboxMessage configuration
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        // DeadLetterMessage configuration
        modelBuilder.ApplyConfiguration(new DeadLetterMessageConfiguration());

        // FeatureFlag configuration
        modelBuilder.ApplyConfiguration(new FeatureFlagConfiguration());

        // ProjectTeam configuration
        modelBuilder.ApplyConfiguration(new ProjectTeamConfiguration());

        // TaskBoardReadModel configuration
        modelBuilder.ApplyConfiguration(new TaskBoardReadModelConfiguration());

        // SprintSummaryReadModel configuration
        modelBuilder.ApplyConfiguration(new SprintSummaryReadModelConfiguration());

        // ProjectOverviewReadModel configuration
        modelBuilder.ApplyConfiguration(new ProjectOverviewReadModelConfiguration());
        modelBuilder.ApplyConfiguration(new CommentConfiguration());
        modelBuilder.ApplyConfiguration(new MentionConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationPreferenceConfiguration());
        modelBuilder.ApplyConfiguration(new AttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new AIDecisionLogConfiguration());
        modelBuilder.ApplyConfiguration(new AIQuotaConfiguration());
        modelBuilder.ApplyConfiguration(new UserAIQuotaOverrideConfiguration());
        modelBuilder.ApplyConfiguration(new UserAIUsageCounterConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationAIQuotaConfiguration());
        modelBuilder.ApplyConfiguration(new UserAIQuotaConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationPermissionPolicyConfiguration());
        modelBuilder.ApplyConfiguration(new TaskDependencyConfiguration());
        modelBuilder.ApplyConfiguration(new MilestoneConfiguration());
        modelBuilder.ApplyConfiguration(new ReleaseConfiguration());
        modelBuilder.ApplyConfiguration(new QualityGateConfiguration());

        // OrganizationInvitation configuration
        modelBuilder.Entity<OrganizationInvitation>(entity =>
        {
            entity.ToTable("OrganizationInvitations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.Token).IsRequired();
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.OrganizationId).IsRequired();
            entity.Property(e => e.InvitedById).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.ExpiresAt).IsRequired();
            entity.Property(e => e.IsUsed).IsRequired().HasDefaultValue(false);
            
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => new { e.Email, e.OrganizationId, e.IsUsed });
            entity.HasIndex(e => e.ExpiresAt);
        });

        // Note: All FK cascade behaviors are set to Restrict globally above
        // to prevent SQL Server cascade path cycles

        // Note: Global query filters for multi-tenancy are not applied here
        // Query filtering should be handled at the application layer in handlers
        // This avoids compilation issues with service injection in query filters
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set OrganizationId for new entities
        // Resolve CurrentUserService from the service provider
        if (_serviceProvider != null)
        {
            var currentUserService = _serviceProvider.GetService<ICurrentUserService>();
            if (currentUserService != null)
            {
                var organizationId = currentUserService.GetOrganizationId();
                if (organizationId > 0)
                {
                    foreach (var entry in ChangeTracker.Entries())
                    {
                        // Check if entity has OrganizationId property
                        var organizationIdProperty = entry.Entity.GetType().GetProperty("OrganizationId");
                        if (organizationIdProperty != null && organizationIdProperty.PropertyType == typeof(int))
                        {
                            // Only set for new entities (Added state)
                            if (entry.State == EntityState.Added)
                            {
                                var currentValue = (int?)organizationIdProperty.GetValue(entry.Entity);
                                // Only set if not already set (default value is 0)
                                if (currentValue == 0)
                                {
                                    organizationIdProperty.SetValue(entry.Entity, organizationId);
                                }
                            }
                        }
                    }
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

