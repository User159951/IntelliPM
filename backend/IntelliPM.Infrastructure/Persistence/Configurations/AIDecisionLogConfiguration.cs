using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the AIDecisionLog entity.
/// Configures the table structure, indexes, and relationships for AI decision logging.
/// </summary>
public class AIDecisionLogConfiguration : IEntityTypeConfiguration<AIDecisionLog>
{
    public void Configure(EntityTypeBuilder<AIDecisionLog> builder)
    {
        builder.ToTable("AIDecisionLogs");

        builder.HasKey(a => a.Id);

        // Indexes for common queries
        builder.HasIndex(a => a.DecisionId)
            .IsUnique()
            .HasDatabaseName("IX_AIDecisionLogs_DecisionId");

        builder.HasIndex(a => a.OrganizationId)
            .HasDatabaseName("IX_AIDecisionLogs_OrganizationId");

        builder.HasIndex(a => a.CreatedAt)
            .HasDatabaseName("IX_AIDecisionLogs_CreatedAt");

        builder.HasIndex(a => a.DecisionType)
            .HasDatabaseName("IX_AIDecisionLogs_DecisionType");

        builder.HasIndex(a => a.AgentType)
            .HasDatabaseName("IX_AIDecisionLogs_AgentType");

        builder.HasIndex(a => a.Status)
            .HasDatabaseName("IX_AIDecisionLogs_Status");

        builder.HasIndex(a => a.ExecutionStatus)
            .HasDatabaseName("IX_AIDecisionLogs_ExecutionStatus");

        // Index for entity decisions
        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("IX_AIDecisionLogs_EntityType_EntityId");

        // Index for pending approvals (filtered index for performance)
        builder.HasIndex(a => new { a.RequiresHumanApproval, a.ApprovedByHuman, a.CreatedAt })
            .HasFilter("[RequiresHumanApproval] = 1 AND [ApprovedByHuman] IS NULL")
            .HasDatabaseName("IX_AIDecisionLogs_PendingApprovals");

        // Index for organization decisions with date
        builder.HasIndex(a => new { a.OrganizationId, a.CreatedAt })
            .HasDatabaseName("IX_AIDecisionLogs_Organization_CreatedAt");

        // Index for correlation ID (for distributed tracing)
        builder.HasIndex(a => a.CorrelationId)
            .HasDatabaseName("IX_AIDecisionLogs_CorrelationId");

        // Required fields
        builder.Property(a => a.OrganizationId)
            .IsRequired();

        builder.Property(a => a.DecisionId)
            .IsRequired();

        builder.Property(a => a.DecisionType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.AgentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.EntityName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Question)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.Decision)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Reasoning)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.ModelName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ModelVersion)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.InputData)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.OutputData)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.AlternativesConsidered)
            .IsRequired()
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("[]");

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        // Decimal precision for confidence score (0.0000 to 1.0000)
        builder.Property(a => a.ConfidenceScore)
            .HasPrecision(5, 4);

        // Cost tracking
        builder.Property(a => a.CostAccumulated)
            .HasPrecision(10, 6)
            .HasDefaultValue(0m);

        // Execution status tracking
        builder.Property(a => a.ExecutionStatus)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Success");

        // Relationships
        builder.HasOne(a => a.RequestedByUser)
            .WithMany()
            .HasForeignKey(a => a.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ApprovedByUser)
            .WithMany()
            .HasForeignKey(a => a.ApprovedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

