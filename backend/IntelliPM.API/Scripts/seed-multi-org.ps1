# PowerShell script to seed multi-organization test data
# Usage: .\seed-multi-org.ps1

param(
    [string]$ConnectionString = ""
)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Multi-Organization Data Seeder" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Get the script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
$apiProject = Join-Path $projectRoot "IntelliPM.API"

if (-not (Test-Path $apiProject)) {
    Write-Host "Error: IntelliPM.API project not found at $apiProject" -ForegroundColor Red
    exit 1
}

Write-Host "Project root: $projectRoot" -ForegroundColor Gray
Write-Host "API project: $apiProject" -ForegroundColor Gray
Write-Host ""

# Check if dotnet is available
try {
    $dotnetVersion = dotnet --version
    Write-Host "Using .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "Error: .NET SDK not found. Please install .NET 8 SDK." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Building project..." -ForegroundColor Yellow
Set-Location $apiProject

# Build the project
$buildResult = dotnet build --no-restore 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green
Write-Host ""

Write-Host "Note: This script requires the application to be running or a direct database connection." -ForegroundColor Yellow
Write-Host "The MultiOrgDataSeeder can be called from:" -ForegroundColor Yellow
Write-Host "  1. Program.cs during application startup" -ForegroundColor Gray
Write-Host "  2. A custom console command" -ForegroundColor Gray
Write-Host "  3. A test project" -ForegroundColor Gray
Write-Host ""

Write-Host "To use the seeder, inject MultiOrgDataSeeder in your code and call SeedAsync()" -ForegroundColor Cyan
Write-Host ""

# Example code snippet
Write-Host "Example usage in Program.cs:" -ForegroundColor Cyan
Write-Host @"
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<MultiOrgDataSeeder>();
    await seeder.SeedAsync();
}
"@ -ForegroundColor Gray

Write-Host ""
Write-Host "Or create a console command:" -ForegroundColor Cyan
Write-Host "dotnet run --project IntelliPM.API -- seed-multi-org" -ForegroundColor Gray

