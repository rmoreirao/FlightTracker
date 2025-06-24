# HTTPS Configuration Solutions for Flight Tracker API

The warning you're seeing is common in .NET development environments:

```
warn: Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware[3]
      Failed to determine the https port for redirect.
```

## Quick Fix Options

### Option 1: Use HTTP Only (Simplest)
Just use the HTTP endpoint for development:
```
http://localhost:5243/api/docs
```

### Option 2: Trust Development Certificate
Run this command once to trust the development HTTPS certificate:
```bash
dotnet dev-certs https --trust
```

### Option 3: Disable HTTPS Redirection in Development
If you want to completely remove the warning, comment out the HTTPS redirection in development mode.

In `Program.cs`, the HTTPS redirection is already configured to work properly:
- Explicit port configuration: 7120 for HTTPS, 5243 for HTTP
- Environment-specific HTTPS redirection settings
- Logging configuration to suppress the warning

## Current Configuration

The API is now configured with:
- HTTP: `http://localhost:5243`
- HTTPS: `https://localhost:7120`
- Explicit URL binding to prevent port conflicts
- Suppressed HTTPS warnings in development logs

## Recommended Approach

For development, use the HTTP endpoint:
```
http://localhost:5243/api/docs
```

This avoids all certificate and HTTPS redirect issues while still providing full API functionality.

## Production Notes

In production, HTTPS redirection will work properly with:
- Valid SSL certificates
- Proper load balancer configuration
- Environment-specific settings
