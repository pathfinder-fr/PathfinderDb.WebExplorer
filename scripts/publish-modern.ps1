[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$OutputPath = "",

    [string]$Runtime = "",

    [switch]$SelfContained,

    [switch]$NoRestore
)

$ErrorActionPreference = "Stop"

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$projectPath = Join-Path $repositoryRoot "src\PathfinderDb.Modern\PathfinderDb.Web\PathfinderDb.Web.csproj"

if (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) {
    throw "Modern web project was not found: $projectPath"
}

if ($SelfContained -and [string]::IsNullOrWhiteSpace($Runtime)) {
    throw "-Runtime is required when -SelfContained is specified (for example: win-x64)."
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repositoryRoot "publish\PathfinderDb.Web"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath = Join-Path $repositoryRoot $OutputPath
}

$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)

if (Test-Path -LiteralPath $OutputPath) {
    Write-Host "Cleaning publish directory: $OutputPath"
    Remove-Item -LiteralPath $OutputPath -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

$publishArguments = @(
    "publish",
    $projectPath,
    "--configuration", $Configuration,
    "--output", $OutputPath,
    "--self-contained", ($SelfContained.IsPresent.ToString().ToLowerInvariant())
)

if (-not [string]::IsNullOrWhiteSpace($Runtime)) {
    $publishArguments += @("--runtime", $Runtime)
}

if ($NoRestore) {
    $publishArguments += "--no-restore"
}

Write-Host "Publishing PathfinderDb.Web..."
Write-Host "  Configuration: $Configuration"
Write-Host "  Runtime:       $(if ([string]::IsNullOrWhiteSpace($Runtime)) { "default" } else { $Runtime })"
Write-Host "  Self-contained: $($SelfContained.IsPresent)"
Write-Host "  Output:        $OutputPath"

& dotnet @publishArguments
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$webConfigPath = Join-Path $OutputPath "web.config"
if (-not (Test-Path -LiteralPath $webConfigPath -PathType Leaf)) {
    throw "Publish completed without an IIS web.config: $webConfigPath"
}

Write-Host "Publish completed successfully."
Write-Host "Point IIS to: $OutputPath"
