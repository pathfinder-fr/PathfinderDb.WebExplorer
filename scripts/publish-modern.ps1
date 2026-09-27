[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [string]$OutputPath = "E:\websites\Pathfinder-FR\PathfinderDb.WebExplorer\publish\PathfinderDb.Web",

    [ValidateRange(1, 300)]
    [int]$AppOfflineTimeoutSeconds = 30,

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
    $OutputPath = "E:\websites\Pathfinder-FR\PathfinderDb.WebExplorer\publish\PathfinderDb.Web"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath = Join-Path $repositoryRoot $OutputPath
}

$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

function Get-LockedPublishFiles {
    param([Parameter(Mandatory)][string]$Path)

    $lockedFiles = [System.Collections.Generic.List[string]]::new()
    foreach ($file in Get-ChildItem -LiteralPath $Path -File -Recurse -Force) {
        if ($file.Name -eq "app_offline.htm") {
            continue
        }

        try {
            $stream = [System.IO.File]::Open(
                $file.FullName,
                [System.IO.FileMode]::Open,
                [System.IO.FileAccess]::Read,
                [System.IO.FileShare]::None
            )
            $stream.Dispose()
        }
        catch [System.IO.IOException] {
            $lockedFiles.Add($file.FullName)
        }
    }

    return $lockedFiles.ToArray()
}

function Wait-ForPublishFilesToUnlock {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][int]$TimeoutSeconds
    )

    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    do {
        $lockedFiles = @(Get-LockedPublishFiles -Path $Path)
        if ($lockedFiles.Count -eq 0) {
            return
        }

        Start-Sleep -Milliseconds 250
    } while ([DateTime]::UtcNow -lt $deadline)

    throw "Timed out after $TimeoutSeconds seconds waiting for IIS to release publish files: $($lockedFiles -join ', ')"
}

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
Write-Host "  IIS deployment: app_offline.htm, waiting up to $AppOfflineTimeoutSeconds seconds for file locks"

$appOfflinePath = Join-Path $OutputPath "app_offline.htm"
$hadAppOfflineFile = Test-Path -LiteralPath $appOfflinePath -PathType Leaf
$previousAppOfflineContent = if ($hadAppOfflineFile) {
    [System.IO.File]::ReadAllBytes($appOfflinePath)
}

try {
    [System.IO.File]::WriteAllText(
        $appOfflinePath,
        "<html><body><h1>Site temporairement indisponible</h1><p>Publication en cours.</p></body></html>",
        [System.Text.UTF8Encoding]::new($false)
    )

    Wait-ForPublishFilesToUnlock -Path $OutputPath -TimeoutSeconds $AppOfflineTimeoutSeconds

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
}
finally {
    if ($hadAppOfflineFile) {
        [System.IO.File]::WriteAllBytes($appOfflinePath, $previousAppOfflineContent)
    }
    else {
        Remove-Item -LiteralPath $appOfflinePath -Force -ErrorAction SilentlyContinue
    }
}
