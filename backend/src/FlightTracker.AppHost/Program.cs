var builder = DistributedApplication.CreateBuilder(args);

// Database - PostgreSQL with TimescaleDB extension and development setup
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .WithInitBindMount("../../../scripts/init-db.sql")
    .WithEnvironment("POSTGRES_DB", "flighttracker")
    .WithEnvironment("POSTGRES_USER", "flighttracker") 
    .WithEnvironment("POSTGRES_PASSWORD", "dev_password")
    .AddDatabase("flighttracker");

// Cache - Redis
var redis = builder.AddRedis("redis")
    .WithDataVolume();

// Message Queue - RabbitMQ
var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithDataVolume()
    .WithManagementPlugin();

// API Service with development flags
var apiService = builder.AddProject<Projects.FlightTracker_Api>("api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("AutoMigrateOnStartup", "true")
    .WithEnvironment("SeedTestDataOnStartup", "true")
    .WithEnvironment("USE_REAL_REPOSITORIES", "true")
    .WithExternalHttpEndpoints();

// Data Ingestion Worker
builder.AddProject<Projects.FlightTracker_DataIngestion>("data-ingestion")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithEnvironment("SeedSampleJobs", "true");

// Price Analytics Worker
builder.AddProject<Projects.FlightTracker_PriceAnalytics>("price-analytics")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithEnvironment("GenerateSampleAnalytics", "true");

// Frontend - Next.js Application
var frontend = builder.AddNpmApp("frontend", "../../../frontend", "dev")
    .WithReference(apiService)
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithEnvironment("NEXT_PUBLIC_API_BASE_URL", apiService.GetEndpoint("https"));

builder.Build().Run();
