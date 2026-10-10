$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path

Describe "build.cmd" {
    It "builds the root solution from any working directory and returns dotnet's exit code" {
        $rootScript = Join-Path $repositoryRoot "build.cmd"
        if (-not (Test-Path -LiteralPath $rootScript -PathType Leaf)) {
            $false | Should Be $true
            return
        }

        $checkout = Join-Path $TestDrive "temporary checkout with spaces"
        $tools = Join-Path $checkout "tools"
        New-Item -ItemType Directory -Path $tools -Force | Out-Null
        $scriptCopy = Join-Path $checkout "build.cmd"
        Copy-Item -LiteralPath $rootScript -Destination $scriptCopy
        Set-Content -LiteralPath (Join-Path $checkout "PathfinderDb.sln") -Value "test solution"

        $capturePath = Join-Path $checkout "dotnet arguments.txt"
        Set-Content -LiteralPath (Join-Path $tools "dotnet.cmd") -Value @(
            '@echo off'
            'echo %* > "%BUILD_CAPTURE_FILE%"'
            'exit /b %BUILD_EXIT_CODE%'
        )

        $oldPath = $env:PATH
        $oldCapturePath = $env:BUILD_CAPTURE_FILE
        $oldExitCode = $env:BUILD_EXIT_CODE
        try {
            $env:PATH = "$tools;$oldPath"
            $env:BUILD_CAPTURE_FILE = $capturePath
            $env:BUILD_EXIT_CODE = "37"
            Push-Location $TestDrive
            try {
                & $env:ComSpec /d /c "`"$scriptCopy`""
                $exitCode = $LASTEXITCODE
            }
            finally {
                Pop-Location
            }

            $exitCode | Should Be 37
            $arguments = Get-Content -LiteralPath $capturePath -Raw
            $arguments | Should Match ([regex]::Escape("build `"$checkout\PathfinderDb.sln`" --configuration Release"))
        }
        finally {
            $env:PATH = $oldPath
            $env:BUILD_CAPTURE_FILE = $oldCapturePath
            $env:BUILD_EXIT_CODE = $oldExitCode
        }
    }
}
