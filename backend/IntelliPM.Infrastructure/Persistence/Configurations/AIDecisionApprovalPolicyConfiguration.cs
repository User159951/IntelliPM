using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the AIDecisionApprovalPolicy entity.
/// </summary>
public class AIDecisionApprovalPolicyConfiguration : IEntityTypeConfiguration<AIDecisionApprovalPolicy>
{
    public void Configure(EntityTypeBuilder<AIDecisionApprovalPolicy> builder)
    {
        builder.ToTable("AIDecisionApprovalPolicies");

        builder.HasKey(p => p.Id);

        // Indexes
        builder.HasIndex(p => new { p.OrganizationId, p.DecisionType, p.IsActive })
            .HasDatabaseName("IX_AIDecisionApprovalPolicies_Org_DecisionType_Active");

        builder.HasIndex(p => p.DecisionType)
            .HasDatabaseName("IX_AIDecisionApprovalPolicies_DecisionType");

        builder.HasIndex(p => p.RequiredRole)
            .HasDatabaseName("IX_AIDecisionApprovalPolicies_RequiredRole");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("IX_AIDecisionApprovalPolicies_IsActive");

        // Required fields
        builder.Property(p => p.DecisionType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.RequiredRole)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.IsBlockingIfNotApproved)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(p => p.Organization)
            .WithMany()
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

