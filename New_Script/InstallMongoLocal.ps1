<#
InstallMongoLocal.ps1
- Parameters:
    -InstallDir (optional)
    -MongoInstaller (optional)
    -LogFile (optional)
#>

param (
    [string]$InstallDir = "C:\Program Files\MongoDB\Server\7.0",
    [string]$MongoInstaller = "",
    [string]$LogFile = ""
)

if ($LogFile -ne "") {
    Start-Transcript -Path $LogFile -Force -ErrorAction SilentlyContinue
}

try {
    Write-Host "Starting local MongoDB installation..."

    if ($MongoInstaller -eq "") {
        # try to find MSI in temp folder
        $tmpFiles = Get-ChildItem -Path $env:TEMP -Filter "mongodb-*.msi" -ErrorAction SilentlyContinue
        if ($tmpFiles.Count -gt 0) {
            $MongoInstaller = $tmpFiles[0].FullName
        } else {
            # look in current folder
            $cwdFiles = Get-ChildItem -Path (Split-Path -Path $MyInvocation.MyCommand.Definition) -Filter "mongodb-*.msi" -ErrorAction SilentlyContinue
            if ($cwdFiles.Count -gt 0) {
                $MongoInstaller = $cwdFiles[0].FullName
            }
        }
    }

    if (-not (Test-Path $MongoInstaller)) {
        Write-Error "MongoDB installer not found. Provide -MongoInstaller parameter or place the MSI next to this script."
        exit 1
    }

    Write-Host "Using installer: $MongoInstaller"
    Write-Host "InstallDir: $InstallDir"

    # Install MSI silently
    $msiArgs = "/i `"$MongoInstaller`" INSTALLLOCATION=`"$InstallDir`" /qn /norestart"
    $p = Start-Process -FilePath msiexec.exe -ArgumentList $msiArgs -Wait -PassThru -ErrorAction Stop

    if ($p.ExitCode -ne 0) {
        Write-Error "msiexec returned exit code: $($p.ExitCode)"
        exit $p.ExitCode
    }

    # Ensure bin is on machine PATH
    $binPath = Join-Path $InstallDir "bin"
    $envPath = [System.Environment]::GetEnvironmentVariable("Path", "Machine")
    if ($envPath -notlike "*$binPath*") {
        [System.Environment]::SetEnvironmentVariable("Path", $envPath + ";" + $binPath, "Machine")
        Write-Host "Added $binPath to machine PATH."
    }

    # Create data folder and install service if needed
    $dataDir = "C:\data\db"
    if (-not (Test-Path $dataDir)) {
        New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
        Write-Host "Created data directory: $dataDir"
    }

    $mongod = Join-Path $binPath "mongod.exe"
    if (Test-Path $mongod) {
        # Install service if not installed
        $svc = Get-Service -Name "MongoDB" -ErrorAction SilentlyContinue
        if ($null -eq $svc) {
            & $mongod --install --config "$InstallDir\bin\mongod.cfg" 2>&1 | Write-Host
            Start-Sleep -Seconds 2
        }
        Try { Start-Service -Name "MongoDB" -ErrorAction Stop; Write-Host "Started MongoDB service." } Catch { Write-Warning "Could not start MongoDB service: $_" }
    } else {
        Write-Warning "mongod.exe not found at $mongod"
    }

    Write-Host "Local MongoDB installation completed successfully."
    exit 0
}
catch {
    Write-Error "Installation failed: $_"
    exit 1
}
finally {
    if ($LogFile -ne "") {
        Stop-Transcript -ErrorAction SilentlyContinue
    }
}
