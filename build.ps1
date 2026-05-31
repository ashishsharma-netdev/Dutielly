param(
    [ValidateSet("all", "web", "windows", "android")]
    [string]$Target = "all"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

if ($Target -eq "all" -or $Target -eq "web") {
    dotnet publish Dutielly.Web\Dutielly.Web.csproj -c Release -o artifacts\web
}

if ($Target -eq "all" -or $Target -eq "windows") {
    dotnet publish Dutielly\Dutielly.csproj -c Release -f net9.0-windows10.0.19041.0 -p:WindowsPackageType=None -o artifacts\windows
}

if ($Target -eq "all" -or $Target -eq "android") {
    $androidSdk = Join-Path $root ".android-sdk"
    dotnet publish Dutielly\Dutielly.csproj -c Release -f net9.0-android -p:AndroidSdkDirectory="$androidSdk" -p:AcceptAndroidSDKLicenses=True -o artifacts\android
}

Write-Host "Dutielly build completed." -ForegroundColor Green
