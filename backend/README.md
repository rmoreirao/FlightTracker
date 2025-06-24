# Flight Tracker Backend
====================
This directory contains the backend code for the Flight Tracker application.

# Running the Backend
To run the backend, ensure you have .NET SDK installed and follow these steps:
1. Open a terminal or command prompt.
2. Navigate to the backend directory.
3. Run the following command to start the application:
   ```bash
   dotnet run --project src/FlightTracker.AppHost
   ```

# Accessing the API

## Option 1: Direct API Access (Recommended for Development)
1. Open a terminal or command prompt.
2. Navigate to the backend directory.
3. Run the following command to start the API:
   ```bash
   dotnet run --project src/FlightTracker.Api/FlightTracker.Api.csproj
   ```
4. Once the backend is running, you can access the API at:
   - **HTTP**: http://localhost:5243/api/docs (recommended for development)
   - **HTTPS**: https://localhost:7120/api/docs
   - **Root redirect**: http://localhost:5243/ (redirects to Swagger docs)

## Option 2: Using .NET Aspire AppHost (Full Application)
Run the complete application with all services:
```bash
dotnet run --project src/FlightTracker.AppHost
```

## Troubleshooting

### HTTPS Warnings
If you see warnings about HTTPS redirection, they are harmless in development. To avoid them:
- Use the HTTP URL: http://localhost:5243/api/docs
- Or trust the development certificate: `dotnet dev-certs https --trust`

### Common Issues
- **Port conflicts**: If ports are in use, the app will select different ones. Check the console output.
- **Certificate issues**: Use HTTP in development or run `dotnet dev-certs https --trust`

## Available Endpoints
- **Swagger UI**: `/api/docs`
- **Health Check**: `/api/v1/health`
- **System Info**: `/api/v1/health/info`
- **Flight Search**: `/api/v1/flights/search` 