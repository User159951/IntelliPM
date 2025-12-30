using Microsoft.EntityFrameworkCore;
using IntelliPM.Infrastructure.VectorStore.Entities;
using Pgvector.EntityFrameworkCore;

namespace IntelliPM.Infrastructure.VectorStore;

/// <summary>
/// PostgreSQL DbContext for vector embeddings and agent memory (uses pgvector extension)
/// </summary>
public class VectorDbContext : DbContext
{
    public VectorDbContext(DbContextOptions<VectorDbContext> options) : base(options) { }

    public DbSet<AgentMemoryRecord> AgentMemories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<AgentMemoryRecord>(entity =>
        {
            entity.ToTable("agent_memories");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.ProjectId)
                .HasColumnName("project_id")
                .IsRequired();
            
            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasMaxLength(100)
                .IsRequired();
            
            entity.Property(e => e.Content)
                .HasColumnName("content")
                .IsRequired();
            
            entity.Property(e => e.Embedding)
                .HasColumnName("embedding")
                .HasColumnType("vector(768)") // 768-dimensional vector (OpenAI/Ollama standard)
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .IsRequired();
            
            entity.Property(e => e.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("jsonb");

            // Indexes
            entity.HasIndex(e => e.ProjectId)
                .HasDatabaseName("idx_agent_memories_project_id");
            
            entity.HasIndex(e => e.Type)
                .HasDatabaseName("idx_agent_memories_type");
            
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("idx_agent_memories_created_at");

            // Vector similarity index (IVFFlat for faster approximate search)
            // Note: This will be created via migration or manual SQL
            // CREATE INDEX idx_agent_memories_embedding ON agent_memories USING ivfflat (embedding vector_cosine_ops) WITH (lists = 100);
        });
    }
}

