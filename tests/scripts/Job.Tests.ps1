$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$jobScript = Join-Path $repositoryRoot "job.cmd"

function Test-JobScriptExists {
    if (-not (Test-Path -LiteralPath $jobScript -PathType Leaf)) {
        $false | Should Be $true
        return $false
    }

    return $true
}

function Invoke-JobFrom {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$WorkingDirectory
    )

    Push-Location $WorkingDirectory
    try {
        $command = "`"$Path`""
        $output = & $env:ComSpec /d /c $command 2>&1 | Out-String
        $exitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }

    return [pscustomobject]@{
        ExitCode = $exitCode
        Output = $output
    }
}

function Invoke-Git {
    param(
        [Parameter(Mandatory)][string]$Directory,
        [Parameter(Mandatory)][string[]]$Arguments
    )

    $output = & git -C $Directory @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "git -C `"$Directory`" $($Arguments -join ' ') failed with exit code ${LASTEXITCODE}: $output"
    }

    return $output
}

function New-RemoteCheckout {
    param([Parameter(Mandatory)][string]$Name)

    $remote = Join-Path $TestDrive "$Name.git"
    $seed = Join-Path $TestDrive "$Name-seed"
    $checkout = Join-Path $TestDrive "$Name-checkout"

    & git init --bare $remote 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Could not initialize test remote." }

    & git init $seed 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Could not initialize seed repository." }
    Invoke-Git -Directory $seed -Arguments @("checkout", "-b", "master") | Out-Null
    Invoke-Git -Directory $seed -Arguments @("config", "user.name", "Job Test") | Out-Null
    Invoke-Git -Directory $seed -Arguments @("config", "user.email", "job-test@example.invalid") | Out-Null
    Set-Content -LiteralPath (Join-Path $seed "tracked.txt") -Value "initial"
    Invoke-Git -Directory $seed -Arguments @("add", "tracked.txt") | Out-Null
    Invoke-Git -Directory $seed -Arguments @("commit", "-m", "initial") | Out-Null
    Invoke-Git -Directory $seed -Arguments @("remote", "add", "origin", $remote) | Out-Null
    Invoke-Git -Directory $seed -Arguments @("push", "-u", "origin", "master") | Out-Null
    & git --git-dir $remote symbolic-ref HEAD refs/heads/master
    if ($LASTEXITCODE -ne 0) { throw "Could not set the test remote's default branch." }

    & git clone $remote $checkout 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Could not clone the test remote." }

    Copy-Item -LiteralPath $jobScript -Destination (Join-Path $checkout "job.cmd")
    return [pscustomobject]@{
        Remote = $remote
        Seed = $seed
        Checkout = $checkout
    }
}

function Initialize-FakeGitJob {
    param(
        [Parameter(Mandatory)][string]$Name,
        [string]$Branch = "master",
        [int]$BranchStatus = 0,
        [int]$ResetStatus = 0,
        [int]$PullStatus = 0,
        [int]$PublishStatus = 0
    )

    $root = Join-Path $TestDrive $Name
    $tools = Join-Path $root "tools"
    New-Item -ItemType Directory -Path $tools -Force | Out-Null
    Copy-Item -LiteralPath $jobScript -Destination (Join-Path $root "job.cmd")

    Set-Content -LiteralPath (Join-Path $tools "git.cmd") -Value @(
        '@echo off'
        '>> "%JOB_GIT_CALLS%" echo %*'
        'if /i "%~3"=="rev-parse" ('
        '  echo %JOB_FAKE_BRANCH%'
        '  exit /b %JOB_BRANCH_STATUS%'
        ')'
        'if /i "%~3"=="reset" exit /b %JOB_RESET_STATUS%'
        'if /i "%~3"=="pull" exit /b %JOB_PULL_STATUS%'
        'exit /b 98'
    )
    Set-Content -LiteralPath (Join-Path $root "publish.bat") -Value @(
        '@echo off'
        '>> "%JOB_PUBLISH_CALLS%" echo publish'
        'exit /b %JOB_PUBLISH_STATUS%'
    )

    $env:PATH = "$tools;$env:PATH"
    $env:JOB_GIT_CALLS = Join-Path $root "git-calls.txt"
    $env:JOB_FAKE_BRANCH = $Branch
    $env:JOB_BRANCH_STATUS = "$BranchStatus"
    $env:JOB_RESET_STATUS = "$ResetStatus"
    $env:JOB_PULL_STATUS = "$PullStatus"
    $env:JOB_PUBLISH_STATUS = "$PublishStatus"
    $env:JOB_PUBLISH_CALLS = Join-Path $root "publish-calls.txt"

    return [pscustomobject]@{
        Root = $root
        GitCalls = $env:JOB_GIT_CALLS
        PublishCalls = $env:JOB_PUBLISH_CALLS
    }
}

Describe "job.cmd" {
    BeforeEach {
        $script:originalPath = $env:PATH
        $script:originalJobEnvironment = @{}
        foreach ($name in @(
            "JOB_GIT_CALLS", "JOB_FAKE_BRANCH", "JOB_BRANCH_STATUS",
            "JOB_RESET_STATUS", "JOB_PULL_STATUS", "JOB_PUBLISH_STATUS",
            "JOB_PUBLISH_CALLS"
        )) {
            $script:originalJobEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, "Process")
        }
    }

    AfterEach {
        $env:PATH = $script:originalPath
        foreach ($name in $script:originalJobEnvironment.Keys) {
            [Environment]::SetEnvironmentVariable($name, $script:originalJobEnvironment[$name], "Process")
        }
    }

    It "resets tracked edits, fast-forward updates, and publishes from the repository root when called elsewhere" {
        if (-not (Test-JobScriptExists)) { return }

        $repositories = New-RemoteCheckout -Name "successful-update"
        Copy-Item -LiteralPath $jobScript -Destination (Join-Path $repositories.Checkout "job.cmd")
        Set-Content -LiteralPath (Join-Path $repositories.Checkout "publish.bat") -Value @(
            '@echo off'
            'type "%~dp0tracked.txt" > "%~dp0published-state.txt"'
            'exit /b 0'
        )

        Set-Content -LiteralPath (Join-Path $repositories.Checkout "tracked.txt") -Value "staged local edit"
        Invoke-Git -Directory $repositories.Checkout -Arguments @("add", "tracked.txt") | Out-Null
        Set-Content -LiteralPath (Join-Path $repositories.Checkout "keep-untracked.txt") -Value "keep me"

        Set-Content -LiteralPath (Join-Path $repositories.Seed "tracked.txt") -Value "remote update"
        Invoke-Git -Directory $repositories.Seed -Arguments @("add", "tracked.txt") | Out-Null
        Invoke-Git -Directory $repositories.Seed -Arguments @("commit", "-m", "remote update") | Out-Null
        Invoke-Git -Directory $repositories.Seed -Arguments @("push", "origin", "master") | Out-Null

        $result = Invoke-JobFrom -Path (Join-Path $repositories.Checkout "job.cmd") -WorkingDirectory $TestDrive

        $result.ExitCode | Should Be 0
        (Get-Content -LiteralPath (Join-Path $repositories.Checkout "tracked.txt") -Raw).Trim() | Should Be "remote update"
        (Get-Content -LiteralPath (Join-Path $repositories.Checkout "keep-untracked.txt") -Raw).Trim() | Should Be "keep me"
        (Get-Content -LiteralPath (Join-Path $repositories.Checkout "published-state.txt") -Raw).Trim() | Should Be "remote update"
    }

    It "checks master before resetting and pulling, then publishes without cleaning untracked files" {
        if (-not (Test-JobScriptExists)) { return }

        $fake = Initialize-FakeGitJob -Name "ordered-calls"
        $result = Invoke-JobFrom -Path (Join-Path $fake.Root "job.cmd") -WorkingDirectory $TestDrive

        $result.ExitCode | Should Be 0
        $calls = @(Get-Content -LiteralPath $fake.GitCalls)
        $branchIndex = -1
        $resetIndex = -1
        $pullIndex = -1
        for ($index = 0; $index -lt $calls.Count; $index++) {
            if ($calls[$index] -match "rev-parse --abbrev-ref HEAD") { $branchIndex = $index }
            if ($calls[$index] -match " reset --hard") { $resetIndex = $index }
            if ($calls[$index] -match " pull --ff-only origin master") { $pullIndex = $index }
        }

        ($branchIndex -ge 0) | Should Be $true
        ($resetIndex -gt $branchIndex) | Should Be $true
        ($pullIndex -gt $resetIndex) | Should Be $true
        (Test-Path -LiteralPath $fake.PublishCalls) | Should Be $true
        ($calls -match "\bclean\b").Count | Should Be 0
    }

    It "refuses a non-master checkout before resetting, pulling, or publishing" {
        if (-not (Test-JobScriptExists)) { return }

        $fake = Initialize-FakeGitJob -Name "wrong-branch" -Branch "feature/test"
        $result = Invoke-JobFrom -Path (Join-Path $fake.Root "job.cmd") -WorkingDirectory $TestDrive

        $result.ExitCode | Should Be 1
        $calls = @(Get-Content -LiteralPath $fake.GitCalls)
        ($calls -match " reset --hard").Count | Should Be 0
        ($calls -match " pull --ff-only origin master").Count | Should Be 0
        (Test-Path -LiteralPath $fake.PublishCalls) | Should Be $false
    }

    It "returns a reset failure without pulling or publishing" {
        if (-not (Test-JobScriptExists)) { return }

        $fake = Initialize-FakeGitJob -Name "reset-failure" -ResetStatus 23
        $result = Invoke-JobFrom -Path (Join-Path $fake.Root "job.cmd") -WorkingDirectory $TestDrive

        $result.ExitCode | Should Be 23
        $calls = @(Get-Content -LiteralPath $fake.GitCalls)
        ($calls -match " pull --ff-only origin master").Count | Should Be 0
        (Test-Path -LiteralPath $fake.PublishCalls) | Should Be $false
    }

    It "returns a pull failure without publishing" {
        if (-not (Test-JobScriptExists)) { return }

        $fake = Initialize-FakeGitJob -Name "pull-failure" -PullStatus 19
        $result = Invoke-JobFrom -Path (Join-Path $fake.Root "job.cmd") -WorkingDirectory $TestDrive

        $result.ExitCode | Should Be 19
        (Test-Path -LiteralPath $fake.PublishCalls) | Should Be $false
    }

    It "returns the publisher failure after successful Git commands" {
        if (-not (Test-JobScriptExists)) { return }

        $fake = Initialize-FakeGitJob -Name "publish-failure" -PublishStatus 41
        $result = Invoke-JobFrom -Path (Join-Path $fake.Root "job.cmd") -WorkingDirectory $TestDrive

        $result.ExitCode | Should Be 41
        (Test-Path -LiteralPath $fake.PublishCalls) | Should Be $true
    }

    It "preserves an untracked file and skips publishing when the remote would overwrite it" {
        if (-not (Test-JobScriptExists)) { return }

        $repositories = New-RemoteCheckout -Name "untracked-conflict"
        Copy-Item -LiteralPath $jobScript -Destination (Join-Path $repositories.Checkout "job.cmd")
        Set-Content -LiteralPath (Join-Path $repositories.Checkout "publish.bat") -Value @(
            '@echo off'
            'echo publish >> "%~dp0published.txt"'
            'exit /b 0'
        )
        Set-Content -LiteralPath (Join-Path $repositories.Seed "collision.txt") -Value "remote file"
        Invoke-Git -Directory $repositories.Seed -Arguments @("add", "collision.txt") | Out-Null
        Invoke-Git -Directory $repositories.Seed -Arguments @("commit", "-m", "add conflicting remote file") | Out-Null
        Invoke-Git -Directory $repositories.Seed -Arguments @("push", "origin", "master") | Out-Null
        Set-Content -LiteralPath (Join-Path $repositories.Checkout "collision.txt") -Value "local untracked file"

        $result = Invoke-JobFrom -Path (Join-Path $repositories.Checkout "job.cmd") -WorkingDirectory $TestDrive

        ($result.ExitCode -ne 0) | Should Be $true
        (Get-Content -LiteralPath (Join-Path $repositories.Checkout "collision.txt") -Raw).Trim() | Should Be "local untracked file"
        (Test-Path -LiteralPath (Join-Path $repositories.Checkout "published.txt")) | Should Be $false
    }
}
