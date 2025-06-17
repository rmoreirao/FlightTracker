var builder = DistributedApplication.CreateBuilder(args);

// Database - PostgreSQL with TimescaleDB extension
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin()
    .AddDatabase("flighttracker");

// Cache - Redis
var redis = builder.AddRedis("redis")
    .WithDataVolume();

// Message Queue - RabbitMQ
var rabbitmq = builder.AddRabbitMQ("messaging")
    .WithDataVolume()
    .WithManagementPlugin();

// API Service
var apiService = builder.AddProject<Projects.FlightTracker_Api>("api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithExternalHttpEndpoints();

// Data Ingestion Worker
builder.AddProject<Projects.FlightTracker_DataIngestion>("data-ingestion")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbitmq);

// Price Analytics Worker
builder.AddProject<Projects.FlightTracker_PriceAnalytics>("price-analytics")
    .WithReference(postgres)
    .WithReference(redis)
    .WithReference(rabbitmq);

builder.Build().Run();
