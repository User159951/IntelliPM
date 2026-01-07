using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the WorkflowTransitionRule entity.
/// </summary>
public class WorkflowTransitionRuleConfiguration : IEntityTypeConfiguration<WorkflowTransitionRule>
{
    public void Configure(EntityTypeBuilder<WorkflowTransitionRule> builder)
    {
        builder.ToTable("WorkflowTransitionRules");

        builder.HasKey(r => r.Id);

        // Property configurations
        builder.Property(r => r.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.FromStatus)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.ToStatus)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.AllowedRolesJson)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(r => r.RequiredConditionsJson)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .IsRequired();

        // Indexes
        // Composite index for efficient rule lookup
        builder.HasIndex(r => new { r.EntityType, r.FromStatus, r.ToStatus, r.IsActive })
            .HasDatabaseName("IX_WorkflowTransitionRules_Lookup");

        // Index on EntityType for filtering
        builder.HasIndex(r => r.EntityType)
            .HasDatabaseName("IX_WorkflowTransitionRules_EntityType");
    }
}

