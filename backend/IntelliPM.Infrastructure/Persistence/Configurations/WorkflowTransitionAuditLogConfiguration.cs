using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the WorkflowTransitionAuditLog entity.
/// </summary>
public class WorkflowTransitionAuditLogConfiguration : IEntityTypeConfiguration<WorkflowTransitionAuditLog>
{
    public void Configure(EntityTypeBuilder<WorkflowTransitionAuditLog> builder)
    {
        builder.ToTable("WorkflowTransitionAuditLogs");

        builder.HasKey(a => a.Id);

        // Property configurations
        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.EntityId)
            .IsRequired();

        builder.Property(a => a.FromStatus)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.ToStatus)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.WasAllowed)
            .IsRequired();

        builder.Property(a => a.DenialReason)
            .HasMaxLength(1000);

        builder.Property(a => a.UserRole)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.AttemptedAt)
            .IsRequired();

        // Indexes
        // Index on UserId for user activity queries
        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_WorkflowTransitionAuditLogs_UserId");

        // Index on EntityType and EntityId for entity history queries
        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("IX_WorkflowTransitionAuditLogs_Entity");

        // Index on ProjectId for project-level audit queries
        builder.HasIndex(a => a.ProjectId)
            .HasDatabaseName("IX_WorkflowTransitionAuditLogs_ProjectId");

        // Index on AttemptedAt for time-based queries
        builder.HasIndex(a => a.AttemptedAt)
            .HasDatabaseName("IX_WorkflowTransitionAuditLogs_AttemptedAt");

        // Index on WasAllowed for filtering denied transitions
        builder.HasIndex(a => a.WasAllowed)
            .HasDatabaseName("IX_WorkflowTransitionAuditLogs_WasAllowed");

        // Index on OrganizationId for tenant filtering
        builder.HasIndex(a => a.OrganizationId)
            .HasDatabaseName("IX_WorkflowTransitionAuditLogs_OrganizationId");

        // Relationships
        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Project)
            .WithMany()
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Organization)
            .WithMany()
            .HasForeignKey(a => a.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}

