# Service Discovery Configuration

## Overview

Service discovery has been configured in the FlightTracker ServiceDefaults project to enable services to communicate using logical names instead of hardcoded URLs.

## How It Works

### 1. Service Registration (AppHost)
Services are registered in the AppHost with logical names:
```csharp
var apiService = builder.AddProject<Projects.FlightTracker_Api>("api")
    .WithReference(postgres)
    .WithReference(redis);
```

### 2. Service Discovery Registration (ServiceDefaults)
The `AddServiceDefaults()` method configures service discovery:
```csharp
builder.Services.AddServiceDiscovery(); // Enables service name resolution
```

### 3. Using Service Discovery in HttpClients

When creating HttpClients in your services, use logical service names:

```csharp
// Example: API service calling another service
builder.Services.AddHttpClient<ExternalServiceClient>(client =>
{
    client.BaseAddress = new Uri("https://external-service"); // Logical name
})
.AddServiceDiscoverySupport(); // Custom extension method
```

### 4. Aspire Automatic Resolution

In the Aspire environment:
- `"https://api"` resolves to the actual API service endpoint
- `"https://redis"` resolves to the Redis connection
- `"https://postgres"` resolves to the PostgreSQL connection

## Configuration Options

### appsettings.json (for non-Aspire environments)
```json
{
  "Services": {
    "external-service": {
      "https": [
        "localhost:8080",
        "10.46.24.90:80"
      ]
    }
  }
}
```

### Environment Variables
```bash
Services__external-service__https__0=localhost:8080
Services__external-service__https__1=10.46.24.90:80
```

## Next Steps

1. When the .NET 9 service discovery package matures, the `AddServiceDiscoverySupport()` method can be updated to use the official `AddServiceDiscovery()` extension.

2. Service discovery is automatically configured for all services using `AddServiceDefaults()`.

3. For inter-service communication, use logical service names in HttpClient base addresses.
