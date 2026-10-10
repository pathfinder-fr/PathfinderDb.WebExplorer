$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$publishScript = Join-Path $repositoryRoot "scripts\publish.ps1"
$publishBatch = Join-Path $repositoryRoot "publish.bat"

Describe "publish.ps1 and publish.bat" {
    It "runs the publisher relative to the wrapper, forwards arguments, and returns its exit code" {
        if (-not (Test-Path -LiteralPath $publishBatch -PathType Leaf)) {
            $false | Should Be $true
            return
        }

        $wrapperRoot = Join-Path $TestDrive "wrapper"
        $wrapperScripts = Join-Path $wrapperRoot "scripts"
        $testBatch = Join-Path $wrapperRoot "publish.bat"
        $stubPublisher = Join-Path $wrapperScripts "publish.ps1"
        $capturePath = Join-Path $TestDrive "publisher-arguments.json"
        New-Item -ItemType Directory -Path $wrapperScripts -Force | Out-Null
        Copy-Item -LiteralPath $publishBatch -Destination $testBatch
        @'
param(
    [int]$ExitCode,
    [string]$CapturePath
)

[pscustomobject]@{
    ExitCode = $ExitCode
    CapturePath = $CapturePath
    WorkingDirectory = (Get-Location).Path
} | ConvertTo-Json | Set-Content -LiteralPath $CapturePath
exit $ExitCode
'@ | Set-Content -LiteralPath $stubPublisher

        Push-Location $TestDrive
        try {
            $command = "`"$testBatch`" -ExitCode 37 -CapturePath `"$capturePath`""
            $output = & $env:ComSpec /d /c $command 2>&1 | Out-String
            $wrapperExitCode = $LASTEXITCODE
        }
        finally {
            Pop-Location
        }

        $wrapperExitCode | Should Be 37
        $capture = Get-Content -LiteralPath $capturePath -Raw | ConvertFrom-Json
        $capture.ExitCode | Should Be 37
        $capture.CapturePath | Should Be $capturePath
        [System.IO.Path]::GetFullPath($capture.WorkingDirectory).TrimEnd([System.IO.Path]::DirectorySeparatorChar) |
            Should Be ([System.IO.Path]::GetFullPath($TestDrive).TrimEnd([System.IO.Path]::DirectorySeparatorChar))
    }

    It "creates the output directory and takes the IIS site offline while publishing without deleting existing files" {
        $outputPath = Join-Path $TestDrive "publish"
        $lockedFile = Join-Path $outputPath "keep.txt"
        $lockReadyPath = Join-Path $TestDrive "lock-ready"
        $global:PublishModernTestOutputPath = $outputPath
        $global:PublishModernTestLockedFile = $lockedFile
        $global:PublishModernTestObservedOffline = $false
        $global:PublishModernTestExistingFilePreserved = $false
        $global:PublishModernTestFileUnlocked = $false
        $global:PublishModernTestProjectPath = $null

        function global:dotnet {
            $global:PublishModernTestProjectPath = $args[1]
            $global:PublishModernTestObservedOffline = Test-Path -LiteralPath (Join-Path $global:PublishModernTestOutputPath "app_offline.htm")
            $global:PublishModernTestExistingFilePreserved = Test-Path -LiteralPath (Join-Path $global:PublishModernTestOutputPath "keep.txt")
            try {
                $stream = [System.IO.File]::Open($global:PublishModernTestLockedFile, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::None)
                $stream.Dispose()
                $global:PublishModernTestFileUnlocked = $true
            }
            catch [System.IO.IOException] {
                $global:PublishModernTestFileUnlocked = $false
            }

            Set-Content -LiteralPath (Join-Path $global:PublishModernTestOutputPath "web.config") -Value "<configuration />"
            $global:LASTEXITCODE = 0
        }

        $lockJob = $null
        try {
            New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
            Set-Content -LiteralPath (Join-Path $outputPath "keep.txt") -Value "existing deployment file"

            $lockJob = Start-Job -ArgumentList $lockedFile, $lockReadyPath -ScriptBlock {
                param($Path, $ReadyPath)
                $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::None)
                Set-Content -LiteralPath $ReadyPath -Value "locked"
                Start-Sleep -Milliseconds 1500
                $stream.Dispose()
            }

            $readyDeadline = [DateTime]::UtcNow.AddSeconds(10)
            while (-not (Test-Path -LiteralPath $lockReadyPath) -and [DateTime]::UtcNow -lt $readyDeadline) {
                Start-Sleep -Milliseconds 50
            }
            (Test-Path -LiteralPath $lockReadyPath) | Should Be $true

            $startedAt = [DateTime]::UtcNow
            $publishOutput = (& $publishScript -OutputPath $outputPath -AppOfflineTimeoutSeconds 5 6>&1 | Out-String)
            $elapsed = [DateTime]::UtcNow - $startedAt

            $global:PublishModernTestObservedOffline | Should Be $true
            [System.IO.Path]::GetFullPath($global:PublishModernTestProjectPath) |
                Should Be (Join-Path $repositoryRoot "src\PathfinderDb.Web\PathfinderDb.Web.csproj")
            $global:PublishModernTestExistingFilePreserved | Should Be $true
            $global:PublishModernTestFileUnlocked | Should Be $true
            ($elapsed.TotalMilliseconds -ge 1000) | Should Be $true
            $publishOutput | Should Match ([regex]::Escape("Output:        $outputPath"))
            (Test-Path -LiteralPath (Join-Path $outputPath "web.config")) | Should Be $true
            (Test-Path -LiteralPath (Join-Path $outputPath "app_offline.htm")) | Should Be $false
        }
        finally {
            if ($lockJob) {
                Stop-Job $lockJob -ErrorAction SilentlyContinue
                Remove-Job $lockJob -Force -ErrorAction SilentlyContinue
            }
            Remove-Item Function:\global:dotnet -ErrorAction SilentlyContinue
            Remove-Variable PublishModernTestOutputPath, PublishModernTestLockedFile, PublishModernTestObservedOffline, PublishModernTestExistingFilePreserved, PublishModernTestFileUnlocked, PublishModernTestProjectPath -Scope Global -ErrorAction SilentlyContinue
        }
    }

    It "defaults to the production publish directory" {
        $tokens = $null
        $parseErrors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($publishScript, [ref]$tokens, [ref]$parseErrors)
        $outputPathParameter = $ast.ParamBlock.Parameters |
            Where-Object { $_.Name.VariablePath.UserPath -eq "OutputPath" }

        $outputPathParameter.DefaultValue.Value | Should Be "E:\websites\Pathfinder-FR\PathfinderDb.WebExplorer\publish\PathfinderDb.Web"
    }

    It "restores an existing app_offline file when publishing fails" {
        $outputPath = Join-Path $TestDrive "already-offline"
        $appOfflinePath = Join-Path $outputPath "app_offline.htm"
        $global:PublishModernTestOutputPath = $outputPath
        New-Item -ItemType Directory -Path $outputPath -Force | Out-Null
        Set-Content -LiteralPath $appOfflinePath -Value "maintenance already in progress"

        function global:dotnet {
            throw "simulated dotnet publish failure"
        }

        try {
            $publishFailed = $false
            try {
                & $publishScript -OutputPath $global:PublishModernTestOutputPath -AppOfflineTimeoutSeconds 2
            }
            catch {
                $publishFailed = $_.Exception.Message -eq "simulated dotnet publish failure"
            }

            $publishFailed | Should Be $true
            (Get-Content -LiteralPath $appOfflinePath -Raw).Trim() | Should Be "maintenance already in progress"
        }
        finally {
            Remove-Item Function:\global:dotnet -ErrorAction SilentlyContinue
            Remove-Variable PublishModernTestOutputPath -Scope Global -ErrorAction SilentlyContinue
        }
    }
}
