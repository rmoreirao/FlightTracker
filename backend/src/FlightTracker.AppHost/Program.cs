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


// Database - PostgreSQL with TimescaleDB extension and development setup
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .WithInitBindMount(scriptsPath)
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
    .WaitFor(postgres)
    .WaitFor(redis)
    .WaitFor(rabbitmq)
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
