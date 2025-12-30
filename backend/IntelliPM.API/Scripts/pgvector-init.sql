-- PostgreSQL pgvector initialization script
-- This script sets up the pgvector extension and creates the initial schema

-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Verify extension is installed
SELECT * FROM pg_extension WHERE extname = 'vector';

-- The EF Core migrations will handle table creation, but you can verify with:
-- \dt agent_memories

-- Optional: Create IVFFlat index for faster approximate vector search
-- This should be run AFTER the table is populated with some data
-- CREATE INDEX IF NOT EXISTS idx_agent_memories_embedding_ivfflat 
--   ON agent_memories 
--   USING ivfflat (embedding vector_cosine_ops) 
--   WITH (lists = 100);

-- For small datasets, a simple index works:
-- CREATE INDEX IF NOT EXISTS idx_agent_memories_embedding 
--   ON agent_memories 
--   USING ivfflat (embedding vector_cosine_ops);

-- Grant permissions (adjust as needed)
GRANT ALL PRIVILEGES ON DATABASE intellipm_vector TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO postgres;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO postgres;

-- Display current configuration
SELECT 
    current_database() as database,
    current_user as user,
    version() as pg_version;

-- Show available vector operators
SELECT 
    oprname as operator,
    oprleft::regtype as left_type,
    oprright::regtype as right_type,
    oprresult::regtype as result_type
FROM pg_operator
WHERE oprname IN ('<->', '<=>', '<#>');

COMMENT ON EXTENSION vector IS 'Vector similarity search for PostgreSQL';

-- Success message
DO $$
BEGIN
    RAISE NOTICE 'pgvector extension initialized successfully!';
    RAISE NOTICE 'Database: intellipm_vector';
    RAISE NOTICE 'Ready for EF Core migrations.';
END $$;

