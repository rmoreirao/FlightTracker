# How to Access Swagger UI - Flight Tracker API

## Quick Start

1. **Start the API**:
   ```bash
   cd d:\repos\FlightTracker\backend
   dotnet run --project src/FlightTracker.Api/FlightTracker.Api.csproj
   ```

2. **Access Swagger UI**:
   - **HTTP**: http://localhost:5243/api/docs
   - **HTTPS**: https://localhost:7120/api/docs
   - **Root redirect**: http://localhost:5243/ (redirects to Swagger)
   - **Short redirect**: http://localhost:5243/docs (redirects to Swagger)

## Troubleshooting

### Common Issues and Solutions

1. **Wrong URL Format**:
   - ❌ `http://localhost:5243/api/docs/index.html`
   - ✅ `http://localhost:5243/api/docs`

2. **Port Not Available**:
   - If port 5243 is in use, the app will use a different port
   - Check the console output when starting the app for the actual URLs

3. **HTTPS Certificate Issues**:
   - Use HTTP version: `http://localhost:5243/api/docs`
   - Or trust the development certificate: `dotnet dev-certs https --trust`

4. **Swagger Not Loading**:
   - Ensure you're running in Development environment
   - Check that the app started successfully without errors

## Available Endpoints in Swagger

- **Health Check**: `GET /api/v1/health`
- **System Info**: `GET /api/v1/health/info`
- **Flight Search**: `GET /api/v1/flights/search`

## API Specification URLs

- **Swagger JSON**: http://localhost:5243/api/docs/v1/swagger.json
- **OpenAPI 3.0 Spec**: Same as above

## Testing the API

1. **Via Swagger UI**:
   - Navigate to http://localhost:5243/api/docs
   - Click "Try it out" on any endpoint
   - Fill in parameters and click "Execute"

2. **Via curl**:
   ```bash
   # Health check
   curl http://localhost:5243/api/v1/health
   
   # System info
   curl http://localhost:5243/api/v1/health/info
   
   # Flight search (example)
   curl "http://localhost:5243/api/v1/flights/search?originCode=NYC&destinationCode=LAX&departureDate=2025-07-01"
   ```

3. **Via browser**:
   - Health check: http://localhost:5243/api/v1/health
   - System info: http://localhost:5243/api/v1/health/info

## Development Notes

- Swagger UI is only available in Development environment
- Custom CSS styling is applied for Flight Tracker branding
- XML documentation is included for comprehensive API docs
- All endpoints include proper error response documentation
