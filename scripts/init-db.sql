-- Initialize ChatSystemDb for StockChatRoomWeb
-- This script runs when the PostgreSQL container starts for the first time

-- Ensure the database is created (should already be done by POSTGRES_DB)
-- CREATE DATABASE "ChatSystemDb" OWNER stockuser;

-- Grant all privileges to stockuser
GRANT ALL PRIVILEGES ON DATABASE "ChatSystemDb" TO stockuser;

-- Connect to the ChatSystemDb database
\c "ChatSystemDb";

-- Grant schema permissions
GRANT ALL ON SCHEMA public TO stockuser;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO stockuser;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO stockuser;

-- Create extensions if needed
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Note: Entity Framework will handle table creation through migrations