using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the DeadLetterMessage entity.
/// Configures the table structure, constraints, and indexes for the Dead Letter Queue.
/// </summary>
public class DeadLetterMessageConfiguration : IEntityTypeConfiguration<DeadLetterMessage>
{
    public void Configure(EntityTypeBuilder<DeadLetterMessage> builder)
    {
        // Table name
        builder.ToTable("DeadLetterMessages");

        // Primary key
        builder.HasKey(d => d.Id);

        // OriginalMessageId: Required, indexed for lookups
        builder.Property(d => d.OriginalMessageId)
            .IsRequired();

        // EventType: Required, MaxLength(200)
        builder.Property(d => d.EventType)
            .IsRequired()
            .HasMaxLength(200);

        // Payload: Required, column type NVARCHAR(MAX) for SQL Server
        builder.Property(d => d.Payload)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        // OriginalCreatedAt: Required
        builder.Property(d => d.OriginalCreatedAt)
            .IsRequired();

        // MovedToDlqAt: Required, default value GETUTCDATE()
        builder.Property(d => d.MovedToDlqAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // TotalRetryAttempts: Required, default 0
        builder.Property(d => d.TotalRetryAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        // LastError: Required, MaxLength(2000)
        builder.Property(d => d.LastError)
            .IsRequired()
            .HasMaxLength(2000);

        // IdempotencyKey: Optional, MaxLength(100), indexed
        builder.Property(d => d.IdempotencyKey)
            .IsRequired(false)
            .HasMaxLength(100);

        // Index on OriginalMessageId for lookups
        builder.HasIndex(d => d.OriginalMessageId)
            .HasDatabaseName("IX_DeadLetterMessages_OriginalMessageId")
            .IsUnique();

        // Index on MovedToDlqAt for querying by date
        builder.HasIndex(d => d.MovedToDlqAt)
            .HasDatabaseName("IX_DeadLetterMessages_MovedToDlqAt");

        // Index on EventType for filtering by event type
        builder.HasIndex(d => d.EventType)
            .HasDatabaseName("IX_DeadLetterMessages_EventType");

        // Index on IdempotencyKey for idempotency checks
        builder.HasIndex(d => d.IdempotencyKey)
            .HasDatabaseName("IX_DeadLetterMessages_IdempotencyKey")
            .HasFilter("[IdempotencyKey] IS NOT NULL");

        // Composite index for common query pattern: messages ordered by date moved to DLQ
        builder.HasIndex(d => new { d.MovedToDlqAt, d.EventType })
            .HasDatabaseName("IX_DeadLetterMessages_MovedToDlqAt_EventType");
    }
}

