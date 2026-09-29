$ErrorActionPreference = "Stop"

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupFile = "Nook-$timestamp.bak"
$containerPath = "/var/opt/mssql/backup/$backupFile"
$hostPath = Join-Path $PSScriptRoot "backups\$backupFile"

Write-Host "Backing up Nook database to $backupFile..."

$sql = "BACKUP DATABASE [Nook] TO DISK = N'$containerPath' WITH INIT, CHECKSUM, STATS = 10;"

docker exec `
    -e "SQL_QUERY=$sql" `
    nook-db `
    bash -lc '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -b -Q "$SQL_QUERY"'

if ($LASTEXITCODE -ne 0) {
    throw "Database backup failed."
}

Write-Host "Verifying backup..."

$verifySql = "RESTORE VERIFYONLY FROM DISK = N'$containerPath' WITH CHECKSUM;"

docker exec `
    -e "SQL_QUERY=$verifySql" `
    nook-db `
    bash -lc '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -b -Q "$SQL_QUERY"'

if ($LASTEXITCODE -ne 0) {
    throw "Database backup verification failed."
}

if (-not (Test-Path $hostPath)) {
    throw "Backup completed in SQL Server but the backup file was not found on the host."
}

$backup = Get-Item $hostPath

Write-Host ""
Write-Host "Backup successful:"
Write-Host $backup.FullName
Write-Host "$([math]::Round($backup.Length / 1MB, 2)) MB"