using IntelliPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IntelliPM.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Mention entity.
/// Configures the table structure, indexes, and relationships for mentions.
/// </summary>
public class MentionConfiguration : IEntityTypeConfiguration<Mention>
{
    public void Configure(EntityTypeBuilder<Mention> builder)
    {
        builder.ToTable("Mentions");

        builder.HasKey(m => m.Id);

        // Indexes for queries
        builder.HasIndex(m => m.CommentId)
            .HasDatabaseName("IX_Mentions_CommentId");

        builder.HasIndex(m => m.MentionedUserId)
            .HasDatabaseName("IX_Mentions_MentionedUserId");

        builder.HasIndex(m => m.OrganizationId)
            .HasDatabaseName("IX_Mentions_OrganizationId");

        // Index for finding unnotified mentions (filtered index for performance)
        builder.HasIndex(m => new { m.NotificationSent, m.CreatedAt })
            .HasFilter("[NotificationSent] = 0")
            .HasDatabaseName("IX_Mentions_NotificationSent_CreatedAt");

        // Required fields
        builder.Property(m => m.CommentId)
            .IsRequired();

        builder.Property(m => m.MentionedUserId)
            .IsRequired();

        builder.Property(m => m.OrganizationId)
            .IsRequired();

        builder.Property(m => m.MentionText)
            .IsRequired()
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(m => m.Comment)
            .WithMany(c => c.Mentions)
            .HasForeignKey(m => m.CommentId)
            .OnDelete(DeleteBehavior.Cascade); // Delete mentions when comment deleted

        builder.HasOne(m => m.MentionedUser)
            .WithMany()
            .HasForeignKey(m => m.MentionedUserId)
            .OnDelete(DeleteBehavior.Cascade); // Delete mentions when user deleted

        // Global query filter to exclude mentions from deleted comments
        // This matches the Comment query filter to prevent loading mentions for deleted comments
        builder.HasQueryFilter(m => !m.Comment.IsDeleted);
    }
}

