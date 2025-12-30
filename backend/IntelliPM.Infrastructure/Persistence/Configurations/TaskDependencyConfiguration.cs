using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the TaskDependency entity.
/// Configures the table structure, indexes, and relationships for task dependencies.
/// </summary>
public class TaskDependencyConfiguration : IEntityTypeConfiguration<TaskDependency>
{
    public void Configure(EntityTypeBuilder<TaskDependency> builder)
    {
        builder.ToTable("TaskDependencies");

        builder.HasKey(td => td.Id);

        // Unique index to prevent duplicate dependencies
        // A task cannot depend on another task with the same dependency type more than once
        builder.HasIndex(td => new { td.SourceTaskId, td.DependentTaskId, td.DependencyType })
            .IsUnique()
            .HasDatabaseName("IX_TaskDependencies_Source_Dependent_Type_Unique");

        // Index on OrganizationId for multi-tenancy filtering
        builder.HasIndex(td => td.OrganizationId)
            .HasDatabaseName("IX_TaskDependencies_OrganizationId");

        // Index on SourceTaskId for queries like "What tasks depend on this task?"
        builder.HasIndex(td => td.SourceTaskId)
            .HasDatabaseName("IX_TaskDependencies_SourceTaskId");

        // Index on DependentTaskId for queries like "What tasks does this task depend on?"
        builder.HasIndex(td => td.DependentTaskId)
            .HasDatabaseName("IX_TaskDependencies_DependentTaskId");

        // Composite index for common query: "Get all dependencies for a source task"
        builder.HasIndex(td => new { td.SourceTaskId, td.OrganizationId })
            .HasDatabaseName("IX_TaskDependencies_SourceTask_Organization");

        // Composite index for common query: "Get all tasks that depend on a specific task"
        builder.HasIndex(td => new { td.DependentTaskId, td.OrganizationId })
            .HasDatabaseName("IX_TaskDependencies_DependentTask_Organization");

        // Required fields
        builder.Property(td => td.SourceTaskId)
            .IsRequired();

        builder.Property(td => td.DependentTaskId)
            .IsRequired();

        builder.Property(td => td.DependencyType)
            .IsRequired()
            .HasConversion<int>(); // Store as int in database

        builder.Property(td => td.OrganizationId)
            .IsRequired();

        builder.Property(td => td.CreatedById)
            .IsRequired();

        builder.Property(td => td.CreatedAt)
            .IsRequired();

        // Relationships
        // SourceTask relationship
        builder.HasOne(td => td.SourceTask)
            .WithMany()
            .HasForeignKey(td => td.SourceTaskId)
            .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete dependencies when task is deleted

        // DependentTask relationship
        builder.HasOne(td => td.DependentTask)
            .WithMany()
            .HasForeignKey(td => td.DependentTaskId)
            .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete dependencies when task is deleted

        // CreatedBy relationship
        builder.HasOne(td => td.CreatedBy)
            .WithMany()
            .HasForeignKey(td => td.CreatedById)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete dependencies when user is deleted
    }
}

