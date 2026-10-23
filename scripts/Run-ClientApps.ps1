<#
.SYNOPSIS
    Launches the eShop.ClientApp desktop, browser (Avalonia WASM), and Android hosts.

.PARAMETER All
    Launch all three targets.

.PARAMETER Desktop
    Launch only the desktop host.

.PARAMETER Browser
    Launch only the browser host on http://localhost:5005.

.PARAMETER Android
    Boot/install the Android emulator and build/install the Android app.

.PARAMETER Avd
    Android Virtual Device name to start (default: Medium_Phone_API_37.0).

.PARAMETER BrowserPort
    Port for the browser dev server (default: 5005).
#>
param(
    [switch]$All,
    [switch]$Desktop,
    [switch]$Browser,
    [switch]$Android,
    [string]$Avd = "Medium_Phone_API_37.0",
    [int]$BrowserPort = 5005
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$appId = "com.companyname.eshop"

$targets = @{}
if ($All) { $targets["Desktop"] = $true; $targets["Browser"] = $true; $targets["Android"] = $true }
else {
    if ($Desktop) { $targets["Desktop"] = $true }
    if ($Browser)  { $targets["Browser"]  = $true }
    if ($Android)  { $targets["Android"] = $true }
}
if (-not $targets.Count) { Write-Host "No target selected. Use -All, -Desktop, -Browser, or -Android."; exit 1 }

Add-Type -Namespace Win32 -Name Window -MemberDefinition '
    [DllImport("user32.dll")] public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
' -ErrorAction SilentlyContinue | Out-Null

if ($targets.Desktop) {
    $existingDesktop = Get-Process -Name "ClientApp.Desktop" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($existingDesktop) {
        Write-Host "Desktop host already running; bringing window forward..."
        $existingDesktop.Refresh()
        if ($existingDesktop.MainWindowHandle -ne 0) {
            [Win32.Window]::ShowWindowAsync($existingDesktop.MainWindowHandle, 3) | Out-Null
            [Win32.Window]::SetForegroundWindow($existingDesktop.MainWindowHandle) | Out-Null
        }
    } else {
        Write-Host "Starting desktop host..."
        $desktopProc = Start-Process -FilePath "dotnet" -ArgumentList "run --project src/ClientApp.Desktop/ClientApp.Desktop.csproj" -WorkingDirectory $repoRoot -PassThru
        $deadline = (Get-Date).AddSeconds(10)
        do {
            Start-Sleep -Milliseconds 250
            $desktopProc.Refresh()
        } while ($desktopProc.MainWindowHandle -eq 0 -and (Get-Date) -lt $deadline)
        if ($desktopProc.MainWindowHandle -ne 0) {
            [Win32.Window]::ShowWindowAsync($desktopProc.MainWindowHandle, 3) | Out-Null
            [Win32.Window]::SetForegroundWindow($desktopProc.MainWindowHandle) | Out-Null
        }
    }
}

if ($targets.Browser) {
    $browserPortInUse = (Get-NetTCPConnection -LocalPort $BrowserPort -ErrorAction SilentlyContinue | Select-Object -First 1)
    if ($browserPortInUse) {
        Write-Host "Browser dev server is already running on port $BrowserPort; opening tab..."
        Start-Process "http://localhost:$BrowserPort/" | Out-Null
    } else {
        Write-Host "Starting browser on http://localhost:$BrowserPort..."
        Start-Process -FilePath "powershell.exe" -ArgumentList "-NoProfile","-Command", "`$env:ASPNETCORE_URLS='http://localhost:$BrowserPort'; dotnet run --no-launch-profile --project src/ClientApp.Browser/ClientApp.Browser.csproj" -WorkingDirectory $repoRoot -WindowStyle Normal | Out-Null
        Start-Sleep -Seconds 2
        Start-Process "http://localhost:$BrowserPort/" | Out-Null
    }
}

if ($targets.Android) {
    $sdkRoot = "$env:LOCALAPPDATA\Android\Sdk"
    $adb = Join-Path $sdkRoot "platform-tools\adb.exe"
    $emulator = Join-Path $sdkRoot "emulator\emulator.exe"

    Write-Host "Checking for attached Android devices..."
    if (-not (Test-Path $adb)) {
        $adb = "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe"
    }

    function Test-AndroidDeviceReady {
        param([string]$AdbPath)
        $lines = (& $AdbPath devices 2>&1) | Out-String
        return [bool]($lines -match '(?m)^\S+\s+device\s*$')
    }

    if (-not (Test-AndroidDeviceReady -AdbPath $adb)) {
        if (-not (Test-Path $emulator)) {
            throw "Android emulator binary not found at $emulator"
        }

        $runningEmus = Get-Process -Name "emulator*" -ErrorAction SilentlyContinue
        if (-not $runningEmus) {
            Write-Host "Starting emulator '$Avd'..."
            Start-Process -FilePath $emulator -ArgumentList "-avd",$Avd -WindowStyle Normal | Out-Null
        } else {
            Write-Host "An emulator process is already running; waiting for it to connect..."
        }

        $deadline = (Get-Date).AddMinutes(6)
        do {
            Start-Sleep -Seconds 5
            Write-Host "Waiting for emulator..." -NoNewline
            Write-Host ""
        } while (-not (Test-AndroidDeviceReady -AdbPath $adb) -and ((Get-Date) -lt $deadline))
    }

    if (-not (Test-AndroidDeviceReady -AdbPath $adb)) {
        throw "No Android device available after waiting."
    }

    Write-Host "Installing/launching Android app..."
    & dotnet build "src/ClientApp.Android/ClientApp.Android.csproj" -t:Run -f "net10.0-android" | Out-Null

    # The -t:Run sometimes builds but does not start the launcher; force launch.
    Start-Sleep -Seconds 2
    & $adb shell monkey -p $appId -c android.intent.category.LAUNCHER 1 | Out-Null
}

Write-Host ""
Write-Host "Requested targets started:"
foreach ($k in $targets.Keys) { Write-Host " - $k" }
Write-Host ""
Write-Host "Browser URL: http://localhost:$BrowserPort/"