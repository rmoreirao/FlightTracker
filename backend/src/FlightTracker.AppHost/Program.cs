var builder = DistributedApplication.CreateBuilder(args);

// Check if the scripts directory exists
var scriptsPath = "../FlightTracker.Infrastructure/scripts"; 
var scriptsFullPath = Path.GetFullPath(scriptsPath);
if (!Directory.Exists(scriptsFullPath))
{
    throw new DirectoryNotFoundException($"Scripts directory not found at: {scriptsFullPath}");
}

// Check if the init script exists
var initScriptPath = Path.Combine(scriptsFullPath, "init-db.sql");
if (!File.Exists(initScriptPath))
{
    throw new FileNotFoundException($"Init script not found at: {initScriptPath}");
}


// Database - TimescaleDB (PostgreSQL with TimescaleDB extension)
var postgresPassword = builder.AddParameter("postgres-password", value: "dev_password", secret: true);
var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithImage("timescale/timescaledb", "latest-pg17")
    .WithDataVolume()
    .WithEnvironment("POSTGRES_DB", "flighttracker")
    .WithEnvironment("POSTGRES_SHARED_PRELOAD_LIBRARIES", "timescaledb")
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));

var flightDb = postgres.AddDatabase("flighttracker");

// Cache - Redis
var redis = builder.AddRedis("redis")
    .WithDataVolume();

// Message Queue - RabbitMQ
var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithDataVolume()
    .WithManagementPlugin();

// API Service with development flags
var apiService = builder.AddProject<Projects.FlightTracker_Api>("api")
    .WithReference(flightDb)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WaitFor(postgres)
    .WaitFor(redis)
    .WaitFor(rabbitmq)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("DatabaseOptions__AutoMigrateOnStartup", "true")
    .WithEnvironment("DatabaseOptions__SeedTestDataOnStartup", "true")
    .WithExternalHttpEndpoints();

// Data Ingestion Worker
builder.AddProject<Projects.FlightTracker_DataIngestion>("data-ingestion")
    .WithReference(flightDb)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WaitFor(postgres)
    .WaitFor(redis)
    .WaitFor(rabbitmq)
    .WithEnvironment("SeedSampleJobs", "true");

// Price Analytics Worker
builder.AddProject<Projects.FlightTracker_PriceAnalytics>("price-analytics")
    .WithReference(flightDb)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WaitFor(postgres)
    .WaitFor(redis)
    .WaitFor(rabbitmq)
    .WithEnvironment("GenerateSampleAnalytics", "true");

// Frontend - Next.js Application
var frontend = builder.AddNpmApp("frontend", "../../../frontend", "dev")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithEnvironment("NEXT_PUBLIC_API_BASE_URL", apiService.GetEndpoint("https"));

builder.Build().Run();
