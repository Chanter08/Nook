$ErrorActionPreference = "Stop"

function Assert-LastCommandSucceeded($message) {
    if ($LASTEXITCODE -ne 0) {
        throw $message
    }
}

Write-Host ""
Write-Host "=== Nook deployment starting ==="
Write-Host ""

# 1. Pull the new application image
Write-Host "Pulling latest Nook image..."
docker compose pull nook
Assert-LastCommandSucceeded "Failed to pull the latest Nook image."

# 2. Back up the current database
Write-Host ""
Write-Host "Backing up database..."
& "$PSScriptRoot\backup.ps1"

# 3. Stop the currently-running application
# SQL Server stays running.
Write-Host ""
Write-Host "Stopping current Nook container..."
docker compose stop nook
Assert-LastCommandSucceeded "Failed to stop Nook."

# 4. Apply EF Core migrations using the NEW image
Write-Host ""
Write-Host "Applying database migrations..."
docker compose run --rm nook --migrate
Assert-LastCommandSucceeded "Database migration failed. Nook has NOT been restarted."

# 5. Start/recreate Nook using the new image
Write-Host ""
Write-Host "Starting new Nook version..."
docker compose up -d --force-recreate nook
Assert-LastCommandSucceeded "Failed to start Nook."

# 6. Discover the configured host port
$portMapping = docker compose port nook 8080
Assert-LastCommandSucceeded "Could not determine Nook's host port."

$hostPort = ($portMapping.Trim() -split ":")[-1]
$healthUrl = "http://localhost:$hostPort/health"

Write-Host ""
Write-Host "Waiting for Nook health check at $healthUrl..."

# 7. Wait up to ~60 seconds for a healthy app
$healthy = $false

for ($attempt = 1; $attempt -le 30; $attempt++) {
    try {
        $response = Invoke-RestMethod -Uri $healthUrl -TimeoutSec 3

        if ($response.status -eq "healthy") {
            $healthy = $true
            break
        }
    }
    catch {
        # App may still be starting.
    }

    Start-Sleep -Seconds 2
}

if (-not $healthy) {
    Write-Host ""
    Write-Host "Nook failed its health check."
    Write-Host ""
    docker compose logs --tail=100 nook

    throw "Deployment failed health check."
}

Write-Host ""
Write-Host "=== Nook deployment successful ==="
Write-Host "Image:"
docker inspect nook --format "{{.Config.Image}}"

Write-Host ""
Write-Host "Health: $healthUrl"