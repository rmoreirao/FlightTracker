# FlightTracker Development Environment Setup Script
# This script starts the complete development environment using .NET Aspire

Write-Host "🚀 Starting FlightTracker Development Environment" -ForegroundColor Green
Write-Host ""

# Check if .NET is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET Version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found. Please install .NET 8 SDK or later." -ForegroundColor Red
    exit 1
}

# Navigate to the AppHost directory
$appHostPath = Join-Path $PSScriptRoot ".." "src" "FlightTracker.AppHost"
if (-not (Test-Path $appHostPath)) {
    Write-Host "❌ AppHost project not found at: $appHostPath" -ForegroundColor Red
    exit 1
}

Write-Host "📁 Navigating to AppHost directory..." -ForegroundColor Yellow
Set-Location $appHostPath

Write-Host "🔧 Building and starting all services..." -ForegroundColor Yellow
Write-Host ""

# Start the Aspire application
try {
    Write-Host "🌟 Starting .NET Aspire orchestration..." -ForegroundColor Cyan
    Write-Host "This will start:" -ForegroundColor Gray
    Write-Host "  • PostgreSQL with TimescaleDB" -ForegroundColor Gray
    Write-Host "  • Redis Cache" -ForegroundColor Gray
    Write-Host "  • RabbitMQ Message Queue" -ForegroundColor Gray
    Write-Host "  • FlightTracker API" -ForegroundColor Gray
    Write-Host "  • Data Ingestion Worker" -ForegroundColor Gray
    Write-Host "  • Price Analytics Worker" -ForegroundColor Gray
    Write-Host "  • Next.js Frontend" -ForegroundColor Gray
    Write-Host ""
    
    # Run the AppHost
    dotnet run
} catch {
    Write-Host "❌ Failed to start the development environment." -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ Development environment startup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "🔗 Service URLs:" -ForegroundColor Cyan
Write-Host "  • Aspire Dashboard: http://localhost:15000" -ForegroundColor White
Write-Host "  • API (HTTPS): https://localhost:7120" -ForegroundColor White
Write-Host "  • API (HTTP): http://localhost:5243" -ForegroundColor White
Write-Host "  • Frontend: http://localhost:3000" -ForegroundColor White
Write-Host "  • pgAdmin: http://localhost:5050" -ForegroundColor White
Write-Host "  • RabbitMQ Management: http://localhost:15672" -ForegroundColor White
Write-Host ""
Write-Host "💡 Tips:" -ForegroundColor Yellow
Write-Host "  • The Aspire Dashboard shows all service logs and metrics" -ForegroundColor Gray
Write-Host "  • Database is automatically initialized with test data" -ForegroundColor Gray
Write-Host "  • Press Ctrl+C to stop all services" -ForegroundColor Gray
