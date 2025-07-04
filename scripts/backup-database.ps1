# Database Backup Script
param(
    [Parameter(Mandatory=$true)]
    [string]$ServerName,
    
    [Parameter(Mandatory=$true)]
    [string]$DatabaseName,
    
    [Parameter(Mandatory=$true)]
    [string]$BackupPath,
    
    [Parameter(Mandatory=$false)]
    [int]$RetentionDays = 30
)

# Create backup directory if it doesn't exist
if (-not (Test-Path $BackupPath)) {
    New-Item -ItemType Directory -Path $BackupPath -Force
}

# Generate backup filename with timestamp
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = Join-Path $BackupPath "$DatabaseName`_$timestamp.bak"

# Perform backup
try {
    $query = "BACKUP DATABASE [$DatabaseName] TO DISK = N'$backupFile' WITH NOFORMAT, NOINIT, NAME = '$DatabaseName-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10"
    Invoke-Sqlcmd -ServerInstance $ServerName -Query $query -ErrorAction Stop
    
    Write-Host "Backup completed successfully: $backupFile"
    
    # Cleanup old backups
    $cutoffDate = (Get-Date).AddDays(-$RetentionDays)
    Get-ChildItem -Path $BackupPath -Filter "$DatabaseName`_*.bak" | 
    Where-Object { $_.LastWriteTime -lt $cutoffDate } | 
    ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-Host "Deleted old backup: $($_.FullName)"
    }
}
catch {
    Write-Error "Backup failed: $_"
    exit 1
} 