# -----------------------------
# Variables
# -----------------------------
$mongoService = "MongoDB"
$configPath   = "C:\Program Files\MongoDB\Server\7.0\bin\mongod.cfg"
$replSetName  = "rs0"
$mongoShell   = "mongosh"

# -----------------------------
# 1. Backup existing config
# -----------------------------
$backupPath = "$configPath.bak_$(Get-Date -Format 'yyyyMMddHHmmss')"
Copy-Item $configPath $backupPath
Write-Host "Backup created: $backupPath"

# -----------------------------
# 2. Ensure replication config exists
# -----------------------------
$configContent = Get-Content $configPath -Raw

if ($configContent -notmatch "replication:") {
    $replicationBlock = @"
replication:
  replSetName: "$replSetName"
"@

    Add-Content -Path $configPath -Value "`n$replicationBlock"
    Write-Host "Replication config added."
}
else {
    Write-Host "Replication config already exists. Skipping update."
}

# -----------------------------
# 3. Restart MongoDB Service
# -----------------------------
Write-Host "Stopping MongoDB service..."
net stop $mongoService

Write-Host "Starting MongoDB service..."
net start $mongoService

# Wait a bit to ensure MongoDB is fully up
Start-Sleep -Seconds 5

# -----------------------------
# 4. Initiate Replica Set
# -----------------------------
$initCmd = @"
rs.initiate({
  _id: "$replSetName",
  members: [
    { _id: 0, host: "localhost:27017" }
  ]
})
"@

Write-Host "Initializing replica set..."
$initCmd | & $mongoShell

Write-Host "Replica set initialization complete."
