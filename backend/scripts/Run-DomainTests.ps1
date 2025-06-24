# Flight Tracker Domain Testing Script
# This script runs all domain tests with coverage reporting and enforces the 80% coverage gate

param(
    [switch]$GenerateReport = $false,
    [switch]$OpenReport = $false,
    [string]$Filter = "*",
    [switch]$Verbose = $false
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$TestProject = Join-Path $ProjectRoot "tests\FlightTracker.Domain.Tests"
$TestResultsPath = Join-Path $TestProject "TestResults"
$CoverageReportsPath = Join-Path $TestResultsPath "Reports"

Write-Host "🚀 Flight Tracker Domain Test Runner" -ForegroundColor Green
Write-Host "=================================="

# Ensure test results directory exists
if (-not (Test-Path $TestResultsPath)) {
    New-Item -ItemType Directory -Path $TestResultsPath -Force | Out-Null
}

# Clean previous test results
Write-Host "🧹 Cleaning previous test results..." -ForegroundColor Yellow
Get-ChildItem $TestResultsPath -Recurse | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue

try {
    # Build the solution first
    Write-Host "🔨 Building solution..." -ForegroundColor Yellow
    dotnet build "$ProjectRoot\FlightTracker.sln" --configuration Release --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }

    # Run tests with coverage
    Write-Host "🧪 Running domain tests with coverage..." -ForegroundColor Yellow
    
    $testArgs = @(
        "test"
        $TestProject
        "--configuration", "Release"
        "--logger", "trx"
        "--logger", "console;verbosity=normal"
        "--results-directory", $TestResultsPath
        "--collect:`"XPlat Code Coverage`""
        "--filter", "FullyQualifiedName~$Filter"
        "/p:CollectCoverage=true"
        "/p:CoverletOutputFormat=cobertura"
        "/p:CoverletOutput=$TestResultsPath\"
        "/p:Exclude=`"[*]*.Program,[*]*.Startup`""
        "/p:Include=`"[FlightTracker.Domain]*`""
        "/p:Threshold=80"
        "/p:ThresholdType=line"
        "/p:ThresholdStat=total"
    )

    if ($Verbose) {
        $testArgs += "--verbosity", "detailed"
    }

    & dotnet @testArgs
    $testExitCode = $LASTEXITCODE

    # Find the coverage file
    $coverageFiles = Get-ChildItem -Path $TestResultsPath -Filter "coverage.cobertura.xml" -Recurse
    if ($coverageFiles.Count -eq 0) {
        # Try alternative location
        $coverageFiles = Get-ChildItem -Path $TestResultsPath -Filter "*.cobertura.xml" -Recurse
    }

    if ($coverageFiles.Count -gt 0) {
        $coverageFile = $coverageFiles[0].FullName
        Write-Host "📊 Coverage file found: $coverageFile" -ForegroundColor Green

        # Parse coverage percentage
        [xml]$coverageXml = Get-Content $coverageFile
        $coverage = $coverageXml.coverage
        $lineRate = [double]$coverage.'line-rate' * 100
        $branchRate = [double]$coverage.'branch-rate' * 100

        Write-Host ""
        Write-Host "📈 Coverage Results:" -ForegroundColor Cyan
        Write-Host "  Line Coverage:   $([Math]::Round($lineRate, 2))%" -ForegroundColor $(if ($lineRate -ge 80) { "Green" } else { "Red" })
        Write-Host "  Branch Coverage: $([Math]::Round($branchRate, 2))%" -ForegroundColor $(if ($branchRate -ge 70) { "Green" } else { "Yellow" })

        # Generate HTML report if requested
        if ($GenerateReport) {
            Write-Host ""
            Write-Host "📋 Generating HTML coverage report..." -ForegroundColor Yellow
            
            if (-not (Test-Path $CoverageReportsPath)) {
                New-Item -ItemType Directory -Path $CoverageReportsPath -Force | Out-Null
            }

            $reportGenArgs = @(
                "-reports:$coverageFile"
                "-targetdir:$CoverageReportsPath"
                "-reporttypes:Html;JsonSummary"
                "-title:`"Flight Tracker Domain Coverage Report`""
                "-tag:`"$(Get-Date -Format 'yyyy-MM-dd_HH-mm-ss')`""
            )

            & dotnet tool run reportgenerator @reportGenArgs
            
            if ($LASTEXITCODE -eq 0) {
                $reportFile = Join-Path $CoverageReportsPath "index.html"
                Write-Host "✅ HTML report generated: $reportFile" -ForegroundColor Green
                
                if ($OpenReport -and (Test-Path $reportFile)) {
                    Write-Host "🌐 Opening report in browser..." -ForegroundColor Yellow
                    Start-Process $reportFile
                }
            } else {
                Write-Host "⚠️  Failed to generate HTML report" -ForegroundColor Yellow
            }
        }

        # Check coverage gate
        Write-Host ""
        if ($lineRate -ge 80) {
            Write-Host "✅ Coverage gate PASSED (80% threshold met)" -ForegroundColor Green
        } else {
            Write-Host "❌ Coverage gate FAILED (80% threshold not met)" -ForegroundColor Red
            Write-Host "   Current: $([Math]::Round($lineRate, 2))% | Required: 80%" -ForegroundColor Red
        }
    } else {
        Write-Host "⚠️  No coverage file found" -ForegroundColor Yellow
    }

    # Display test results summary
    Write-Host ""
    $trxFiles = Get-ChildItem -Path $TestResultsPath -Filter "*.trx" -Recurse
    if ($trxFiles.Count -gt 0) {
        $trxFile = $trxFiles[0].FullName
        [xml]$trxXml = Get-Content $trxFile
        $results = $trxXml.TestRun.ResultSummary
        $counters = $results.Counters
        
        Write-Host "🧪 Test Results Summary:" -ForegroundColor Cyan
        Write-Host "  Total:    $($counters.total)" -ForegroundColor White
        Write-Host "  Passed:   $($counters.passed)" -ForegroundColor Green
        Write-Host "  Failed:   $($counters.failed)" -ForegroundColor $(if ([int]$counters.failed -eq 0) { "Green" } else { "Red" })
        Write-Host "  Skipped:  $($counters.inconclusive)" -ForegroundColor Yellow
    }

    Write-Host ""
    if ($testExitCode -eq 0) {
        Write-Host "🎉 All tests completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "❌ Some tests failed!" -ForegroundColor Red
    }

} catch {
    Write-Host "💥 Error occurred: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Return appropriate exit code
if ($testExitCode -ne 0) {
    exit $testExitCode
}

# Also fail if coverage is below threshold
if ($coverageFiles.Count -gt 0) {
    [xml]$coverageXml = Get-Content $coverageFiles[0].FullName
    $lineRate = [double]$coverageXml.coverage.'line-rate' * 100
    if ($lineRate -lt 80) {
        Write-Host ""
        Write-Host "❌ Exiting with error due to insufficient coverage" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "✅ All quality gates passed!" -ForegroundColor Green
exit 0
