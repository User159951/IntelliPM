using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the OutboxMessage entity.
/// Configures the table structure, constraints, and indexes for the Outbox pattern.
/// </summary>
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        // Table name
        builder.ToTable("OutboxMessages");

        // Primary key
        builder.HasKey(o => o.Id);

        // EventType: Required, MaxLength(200)
        builder.Property(o => o.EventType)
            .IsRequired()
            .HasMaxLength(200);

        // Payload: Required, column type NVARCHAR(MAX) for SQL Server
        builder.Property(o => o.Payload)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        // CreatedAt: Required, default value GETUTCDATE()
        builder.Property(o => o.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // ProcessedAt: Optional
        builder.Property(o => o.ProcessedAt)
            .IsRequired(false);

        // RetryCount: Required, default 0
        builder.Property(o => o.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Error: Optional, MaxLength(2000)
        builder.Property(o => o.Error)
            .IsRequired(false)
            .HasMaxLength(2000);

        // IdempotencyKey: Optional, MaxLength(100), indexed
        builder.Property(o => o.IdempotencyKey)
            .IsRequired(false)
            .HasMaxLength(100);

        // NextRetryAt: Optional, for exponential backoff retry logic
        builder.Property(o => o.NextRetryAt)
            .IsRequired(false);

        // Index on ProcessedAt for query performance (filtering unprocessed messages)
        builder.HasIndex(o => o.ProcessedAt)
            .HasDatabaseName("IX_OutboxMessages_ProcessedAt");

        // Index on CreatedAt for cleanup queries (finding old processed messages)
        builder.HasIndex(o => o.CreatedAt)
            .HasDatabaseName("IX_OutboxMessages_CreatedAt");

        // Index on IdempotencyKey for idempotency checks
        builder.HasIndex(o => o.IdempotencyKey)
            .HasDatabaseName("IX_OutboxMessages_IdempotencyKey")
            .HasFilter("[IdempotencyKey] IS NOT NULL");

        // Index on NextRetryAt for retry logic queries
        builder.HasIndex(o => o.NextRetryAt)
            .HasDatabaseName("IX_OutboxMessages_NextRetryAt")
            .HasFilter("[NextRetryAt] IS NOT NULL");

        // Composite index for common query pattern: unprocessed messages ordered by creation time
        builder.HasIndex(o => new { o.ProcessedAt, o.CreatedAt })
            .HasDatabaseName("IX_OutboxMessages_ProcessedAt_CreatedAt");

        // Composite index for retry queries: unprocessed messages with NextRetryAt
        builder.HasIndex(o => new { o.ProcessedAt, o.NextRetryAt, o.RetryCount })
            .HasDatabaseName("IX_OutboxMessages_ProcessedAt_NextRetryAt_RetryCount")
            .HasFilter("[ProcessedAt] IS NULL");
    }
}

