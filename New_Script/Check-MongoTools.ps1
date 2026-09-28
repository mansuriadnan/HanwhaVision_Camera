# Check-MongoTools.ps1
# Quick script to verify MongoDB Tools installation

Write-Host "=== MongoDB Tools Installation Check ===" -ForegroundColor Cyan
Write-Host ""

# Function to check registry key
function Test-RegistryKey {
    param($Path)
    try {
        Get-ItemProperty -Path $Path -ErrorAction Stop | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

# Function to get registry value
function Get-RegistryValue {
    param($Path, $Name)
    try {
        $value = (Get-ItemProperty -Path $Path -Name $Name -ErrorAction Stop).$Name
        return $value
    }
    catch {
        return $null
    }
}

# Registry paths to check
$regPaths = @(
    "HKLM:\SOFTWARE\MongoDB\MongoDB Tools 100",
    "HKLM:\SOFTWARE\WOW6432Node\MongoDB\MongoDB Tools 100"
)

# Common file paths to check
$commonPaths = @(
    "${env:ProgramFiles}\MongoDB\Tools\100\bin\mongodump.exe",
    "${env:ProgramFiles(x86)}\MongoDB\Tools\100\bin\mongodump.exe",
    "C:\Program Files\MongoDB\Tools\100\bin\mongodump.exe"
)

$registryFound = $false
$filesFound = $false
$installPath = $null

# Check Registry
Write-Host "1. Checking Registry..." -ForegroundColor Yellow
foreach ($regPath in $regPaths) {
    if (Test-RegistryKey $regPath) {
        Write-Host "   [FOUND] Registry key: $regPath" -ForegroundColor Green
        $registryFound = $true
        
        # Try to get install path
        $possibleKeys = @("InstallPath", "InstallLocation", "(Default)")
        foreach ($key in $possibleKeys) {
            $path = Get-RegistryValue $regPath $key
            if ($path) {
                $installPath = $path
                Write-Host "   Install Path from registry: $installPath" -ForegroundColor Cyan
                break
            }
        }
    }
    else {
        Write-Host "   [NOT FOUND] $regPath" -ForegroundColor Gray
    }
}

if (-not $registryFound) {
    Write-Host "   No MongoDB Tools registry entries found!" -ForegroundColor Red
}

Write-Host ""

# Check Files
Write-Host "2. Checking Files..." -ForegroundColor Yellow

# Check registry-based path first
if ($installPath) {
    $exePath = Join-Path $installPath "bin\mongodump.exe"
    if (Test-Path $exePath) {
        Write-Host "   [FOUND] MongoDB Tools executable: $exePath" -ForegroundColor Green
        $filesFound = $true
        
        # Get file version
        $fileInfo = Get-Item $exePath
        Write-Host "   Version: $($fileInfo.VersionInfo.FileVersion)" -ForegroundColor Cyan
        Write-Host "   Size: $([math]::Round($fileInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
    }
    else {
        Write-Host "   [NOT FOUND] Expected executable not found: $exePath" -ForegroundColor Red
    }
}

# Check common paths
foreach ($path in $commonPaths) {
    if (Test-Path $path) {
        Write-Host "   [FOUND] MongoDB Tools executable: $path" -ForegroundColor Green
        $filesFound = $true
        
        # Get file version
        $fileInfo = Get-Item $path
        Write-Host "   Version: $($fileInfo.VersionInfo.FileVersion)" -ForegroundColor Cyan
        Write-Host "   Size: $([math]::Round($fileInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan
    }
    else {
        Write-Host "   [NOT FOUND] $path" -ForegroundColor Gray
    }
}

if (-not $filesFound) {
    Write-Host "   No MongoDB Tools executables found!" -ForegroundColor Red
}

Write-Host ""

# Final verdict
Write-Host "=== RESULT ===" -ForegroundColor Cyan
if ($registryFound -and $filesFound) {
    Write-Host "MongoDB Tools 100 is PROPERLY INSTALLED" -ForegroundColor Green
}
elseif ($registryFound -and -not $filesFound) {
    Write-Host "WARNING: Registry entries exist but files are MISSING!" -ForegroundColor Red
    Write-Host "This is likely a leftover from incomplete uninstall." -ForegroundColor Yellow
}
elseif (-not $registryFound -and $filesFound) {
    Write-Host "WARNING: Files exist but registry entries are MISSING!" -ForegroundColor Yellow
}
else {
    Write-Host "MongoDB Tools 100 is NOT INSTALLED" -ForegroundColor Red
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")