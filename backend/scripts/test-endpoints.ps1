# Test script for FlightTracker API endpoints
param(
    [string]$BaseUrl = "https://localhost:7120"
)

Write-Host "🧪 Testing FlightTracker API endpoints..." -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Cyan

# Function to test endpoint
function Test-Endpoint {
    param(
        [string]$Url,
        [string]$Name
    )
    
    try {
        Write-Host "`n🔍 Testing $Name..." -ForegroundColor Yellow
        Write-Host "   URL: $Url" -ForegroundColor Gray
        
        $response = Invoke-RestMethod -Uri $Url -Method Get -SkipCertificateCheck
        
        # Format response based on endpoint type
        if ($Name -like "*Health*") {
            $summary = "Status: $($response.status), Version: $($response.version), Environment: $($response.environment)"
        }
        elseif ($Name -like "*Flight Search*") {
            $flightCount = if ($response.flights) { $response.flights.Count } else { 0 }
            $summary = "Found $flightCount flights, Currency: $($response.currency), Last Updated: $($response.lastUpdated)"
        }
        elseif ($Name -like "*Flight Details*") {
            $summary = "Flight: $($response.flightNumber), From: $($response.origin.code) to $($response.destination.code), Price: $($response.price.amount) $($response.price.currency)"
        }
        else {
            $summary = $($response | ConvertTo-Json -Depth 1 -Compress)
        }
        
        Write-Host "   ✅ Success! $summary" -ForegroundColor Green
        return $true
    }
    catch {
        $errorMessage = $_.Exception.Message
        if ($_.Exception.Response) {
            $statusCode = $_.Exception.Response.StatusCode
            Write-Host "   ❌ Failed: HTTP $statusCode - $errorMessage" -ForegroundColor Red
        } else {
            Write-Host "   ❌ Failed: $errorMessage" -ForegroundColor Red
        }
        return $false
    }
}

# Test endpoints
$endpoints = @{
    "Health Check" = "$BaseUrl/api/v1/Health"
    "System Info" = "$BaseUrl/api/v1/Health/info"
    "Flight Search - JFK to LAX" = "$BaseUrl/api/v1/Flights/search?originCode=JFK&destinationCode=LAX&departureDate=2025-07-20&adults=1"
    "Flight Search - NYC to SF (with return)" = "$BaseUrl/api/v1/Flights/search?originCode=JFK&destinationCode=SFO&departureDate=2025-07-25&returnDate=2025-07-30&adults=2&cabins=Economy,Business"
    "Flight Search - Business Class" = "$BaseUrl/api/v1/Flights/search?originCode=LAX&destinationCode=ORD&departureDate=2025-08-01&adults=1&cabins=Business"
    "Flight Details - Sample Flight" = "$BaseUrl/api/v1/Flights/AA/AA1001?departureDate=2025-07-20"
}

$successCount = 0
$totalCount = $endpoints.Count

foreach ($endpoint in $endpoints.GetEnumerator()) {
    if (Test-Endpoint -Url $endpoint.Value -Name $endpoint.Key) {
        $successCount++
    }
    Start-Sleep -Milliseconds 500
}

Write-Host "`n📊 Test Results:" -ForegroundColor Cyan
Write-Host "   Successful: $successCount/$totalCount" -ForegroundColor $(if($successCount -eq $totalCount) { "Green" } else { "Yellow" })
Write-Host "   Success Rate: $([math]::Round(($successCount / $totalCount) * 100, 2))%" -ForegroundColor $(if($successCount -eq $totalCount) { "Green" } else { "Yellow" })

if ($successCount -eq $totalCount) {
    Write-Host "`n🎉 All tests passed! API is working correctly." -ForegroundColor Green
} else {
    Write-Host "`n⚠️  Some tests failed. Check the API logs for details." -ForegroundColor Yellow
}
