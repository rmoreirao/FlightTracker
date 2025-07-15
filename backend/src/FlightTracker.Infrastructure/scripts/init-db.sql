-- FlightTracker Database Initialization Script
-- This script sets up TimescaleDB and development configurations

-- Enable TimescaleDB extension
CREATE EXTENSION IF NOT EXISTS timescaledb;

-- Enable additional extensions for development
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS pg_stat_statements;

-- Create application user if not exists
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'flighttracker') THEN
        CREATE USER flighttracker WITH PASSWORD 'dev_password';
    END IF;
END
$$;

-- Grant necessary permissions
GRANT ALL PRIVILEGES ON DATABASE flighttracker TO flighttracker;
ALTER USER flighttracker CREATEDB;

-- Development-specific settings for better debugging
ALTER SYSTEM SET log_statement = 'mod';
ALTER SYSTEM SET log_min_duration_statement = 100;
ALTER SYSTEM SET log_connections = 'on';
ALTER SYSTEM SET log_disconnections = 'on';

-- Apply settings
SELECT pg_reload_conf();

-- Create indexes that will be useful for development queries
-- Note: These will be created after tables exist via migrations
