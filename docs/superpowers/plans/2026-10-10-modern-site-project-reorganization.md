# Modern Site Project Reorganization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the .NET 10 Pathfinder site the repository’s clear current application and provide tested root-level Windows commands for building, publishing, and scheduled Git-update deployment to IIS.

**Architecture:** Promote `PathfinderDb.Data` and `PathfinderDb.Web` to direct children of `src`, and make a new root solution the canonical solution. Preserve the MVC 4 project and its relative build layout under `archive/legacy`. Keep IIS publish logic in a tested PowerShell script, with root `.cmd` launchers for direct publish and the scheduled reset/pull/publish job.

**Tech Stack:** .NET 10, ASP.NET Core Razor Pages, Windows command scripts, PowerShell, xUnit, Pester, Git.

**Spec:** `docs/superpowers/specs/2026-10-10-modern-site-project-reorganization-design.md`

## Global Constraints

- The clone of the server is `E:\websites\Pathfinder-FR\PathfinderDb.WebExplorer`.
- The IIS physical path remains `E:\websites\Pathfinder-FR\PathfinderDb.WebExplorer\publish\PathfinderDb.Web`.
- The job uses `origin/master` and `git pull --ff-only`.
- Before the pull, the job resets tracked changes with `git reset --hard`.
- Untracked files are preserved; `git clean` is never run.
- The job refuses to run unless the current branch is `master`.
- Keep the current `app_offline.htm` and IIS file-unlock behavior during publishing.
- Keep the .NET 10 framework-dependent deployment and external `pf1-data` clone.
- Do not remove legacy sources, their XML data, or historical dated plans/specs.
- No script may delete the publish directory or deploy repository/data exports into it.

## Review Focus

- Scheduler uses a different working directory or paths with spaces; scripts still resolve the checkout from their own location. **Test in Tasks 2–4:** invoke copied root scripts from an unrelated directory and use temporary paths with spaces.
- Job starts on a branch other than `master`; it exits before reset, pull, or publication. **Test in Task 4:** stub the branch query and assert no later command is called.
- Tracked local edits and staged changes are discarded, while untracked files are never removed. **Test in Task 4:** run against an isolated temporary Git clone with both tracked and untracked changes.
- Reset or pull fails (including an untracked-file conflict); the job does not publish and returns nonzero. **Test in Task 4:** simulate each Git failure and assert the publisher was not invoked.
- Publish fails after the app is taken offline; the previous offline marker is restored and the failure reaches the scheduler. **Test in Tasks 3–4:** retain the existing Pester failure test and assert the batch wrapper’s exit code.

---

### Task 1: Promote the .NET 10 projects and archive the MVC project

**Files:**
- Move: `src/PathfinderDb.Modern/PathfinderDb.Data/` → `src/PathfinderDb.Data/`
- Move: `src/PathfinderDb.Modern/PathfinderDb.Web/` → `src/PathfinderDb.Web/`
- Move: `src/PathfinderDb.WebExplorer/` → `archive/legacy/src/PathfinderDb.WebExplorer/`
- Move: `PathfinderDb.WebExplorer.sln` → `archive/legacy/PathfinderDb.WebExplorer.sln`
- Move: `build/` → `archive/legacy/build/`
- Move: old root `build.cmd` → `archive/legacy/build.cmd`
- Move: `src/PathfinderDb.Modern/PathfinderDb.Modern.sln` → root `PathfinderDb.sln`
- Modify: `tests/PathfinderDb.Modern.Tests/PathfinderDb.Modern.Tests.csproj`
- Modify: `tests/PathfinderDb.Modern.IntegrationTests/PathfinderDb.Modern.IntegrationTests.csproj`
- Modify: `tests/PathfinderDb.Modern.Tests/CatalogRouteTests.cs`
- Modify: `tests/PathfinderDb.Modern.Tests/CatalogSummaryRenderingTests.cs`
- Modify: `tests/PathfinderDb.Modern.Tests/HomepageRenderingTests.cs`

**Interfaces:**
- Consumes: existing `PathfinderDb.Data`, `PathfinderDb.Web`, unit/integration test projects, and legacy solution/build wrapper.
- Produces: root `PathfinderDb.sln` referencing `src/PathfinderDb.Data/PathfinderDb.Data.csproj`, `src/PathfinderDb.Web/PathfinderDb.Web.csproj`, and both existing test projects.

- [ ] **Step 1: Relocate the legacy project while preserving its build-relative layout**

Use Git moves so history follows the source. Keep the legacy source below `archive/legacy/src`, the old solution at `archive/legacy`, the old build project below `archive/legacy/build`, and the old build wrapper at `archive/legacy/build.cmd`; these locations preserve the legacy solution and wrapper’s existing relative references.

- [ ] **Step 2: Relocate the modern projects and promote their solution**

Move the data and web project directories to direct children of `src`. Move the modern solution to root as `PathfinderDb.sln`; update its project entries to the new `src` and `tests` paths.

- [ ] **Step 3: Update project references and source-file test paths**

Change the two modern test project references to `..\src\PathfinderDb.Data\...` and, for unit tests, `..\src\PathfinderDb.Web\...`. Update `CatalogRouteTests`, `CatalogSummaryRenderingTests`, and `HomepageRenderingTests` so every source-file read resolves `src/PathfinderDb.Web/...` from the repository root. Keep existing test project names and directories.

- [ ] **Step 4: Build and run tests from the new layout**

Run: `dotnet build PathfinderDb.sln --configuration Release`

Expected: all four modern projects build and the archived legacy source remains present.

Run: `dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj`

Expected: all modern unit tests pass.

Run: `$env:PATHFINDER_DATA_ROOT = 'D:\code\perso\pf\pf1-data'; dotnet test tests\PathfinderDb.Modern.IntegrationTests\PathfinderDb.Modern.IntegrationTests.csproj`

Expected: real-data integration tests pass when the local clone is available; otherwise report the missing opt-in prerequisite without changing the repository.

---

### Task 2: Make root build and current documentation point to the modern site

**Files:**
- Create: `build.cmd`
- Create: `tests/scripts/Build.Tests.ps1`
- Create: `archive/legacy/README.md`
- Modify: `README.md`
- Modify: `.github/copilot-instructions.md`
- Modify: `docs/modern-data-pipeline.md`

**Interfaces:**
- Consumes: root `PathfinderDb.sln` from Task 1.
- Produces: a root `build.cmd` that builds `PathfinderDb.sln` in Release and returns `dotnet`’s exit code.

- [ ] **Step 1: Write Pester tests for the root build wrapper**

Create `tests/scripts/Build.Tests.ps1`. Assert that `build.cmd` invokes the root `PathfinderDb.sln` in Release regardless of the caller’s current directory and returns the `dotnet` process’s nonzero exit code. Run a copied wrapper with a fake `dotnet` executable in a temporary directory so the test never builds or edits the real checkout.

- [ ] **Step 2: Run the build-wrapper tests to verify the expected failures**

Run: `Invoke-Pester tests\scripts\Build.Tests.ps1`

Expected: assertions fail because the root wrapper is still the legacy MSBuild command.

- [ ] **Step 3: Replace the root build command with the current solution build**

Create a batch wrapper that calls `dotnet build` on `PathfinderDb.sln` resolved relative to `%~dp0`, uses Release configuration, works from any current directory, and returns the exact nonzero failure code.

- [ ] **Step 4: Keep legacy build guidance with the archived application**

Create `archive/legacy/README.md` with the applicable legacy-project/build information from the current README. Keep the archived `build.cmd`, solution, project, and `build.proj` usable without changing their relative layout.

- [ ] **Step 5: Rewrite current repository guidance for the modern site**

Make root `README.md`, `.github/copilot-instructions.md`, and `docs/modern-data-pipeline.md` identify `src/PathfinderDb.Web` and `src/PathfinderDb.Data` as current. Update root build/test/run commands and explain the scheduler entry point, server checkout, fixed IIS output path, external data configuration, .NET 10 runtime prerequisites, tracked-change reset, and untracked-file behavior. Do not rewrite dated specs or plans.

- [ ] **Step 6: Verify the root build wrapper**

Run `cmd /c "D:\code\perso\pf\PathfinderDb.WebExplorer\build.cmd"` with an unrelated working directory.

Expected: the modern Release solution builds successfully.

---

### Task 3: Add a root direct-publish entry point

**Files:**
- Move: `scripts/publish-modern.ps1` → `scripts/publish.ps1`
- Create: `publish.bat`
- Move: `tests/scripts/PublishModern.Tests.ps1` → `tests/scripts/Publish.Tests.ps1`
- Modify: `tests/scripts/Publish.Tests.ps1`

**Interfaces:**
- Consumes: `src/PathfinderDb.Web/PathfinderDb.Web.csproj` and the existing IIS publish safeguards.
- Produces: `publish.bat`, which invokes `scripts/publish.ps1` relative to itself, forwards arguments, and returns the PowerShell publisher’s exit code.

- [ ] **Step 1: Update the publication tests for the promoted project and root wrapper**

Rename the Pester test file and update its script path. Add tests proving the output defaults to `E:\websites\Pathfinder-FR\PathfinderDb.WebExplorer\publish\PathfinderDb.Web`, the web project path is `src\PathfinderDb.Web\PathfinderDb.Web.csproj`, and the wrapper works from an unrelated current directory, forwards arguments, and returns the PowerShell process’s status.

- [ ] **Step 2: Run the publication tests to verify the new assertions fail**

Run: `Invoke-Pester tests\scripts\Publish.Tests.ps1`

Expected: the new wrapper/path assertions fail before implementation; existing app-offline and failure-restoration assertions continue to exercise the current PowerShell publisher.

- [ ] **Step 3: Update and rename the PowerShell publisher**

Rename it to `scripts/publish.ps1`, resolve the web project under `src\PathfinderDb.Web`, and retain the existing default IIS destination and all file-lock, app-offline restoration, `dotnet publish` exit-code, and `web.config` checks.

- [ ] **Step 4: Implement the root `publish.bat` wrapper**

Invoke `powershell.exe` with `-NoProfile` and `-File "%~dp0scripts\publish.ps1"`, forward optional arguments, and return the child process exit code. Do not perform Git operations in this wrapper.

- [ ] **Step 5: Run the publication test suite**

Run: `Invoke-Pester tests\scripts\Publish.Tests.ps1`

Expected: all direct-publish wrapper, path, offline-marker, output, and error-propagation tests pass.

---

### Task 4: Add and test the scheduled update-and-publish job

**Files:**
- Create: `job.cmd`
- Create: `tests/scripts/Job.Tests.ps1`

**Interfaces:**
- Consumes: root checkout location from `%~dp0`, branch `master`, `origin/master`, and root `publish.bat` from Task 3.
- Produces: a silent/noninteractive scheduler entry point that validates branch, resets tracked changes, fast-forward pulls, calls `publish.bat`, and returns the first failing operation’s error code.

- [ ] **Step 1: Write Pester tests for job order and failure behavior**

For the success case, create a temporary bare Git remote and clone, copy `job.cmd` into that clone, and add a fake `publish.bat`; change and stage a tracked file locally, create an untracked file, and advance the remote. Invoke the copied job from an unrelated current directory. Assert the tracked content matches the pulled remote, the untracked file remains, and publication ran after the update. For failure cases, use a fake `git.cmd` in a temporary test folder to capture calls and return controlled statuses. Assert:
  - branch check for `master` precedes `git reset --hard`;
  - reset precedes `git pull --ff-only origin master`;
  - publication runs only after successful Git commands;
  - a non-`master` branch exits before reset/pull/publication;
  - reset and pull failures prevent publication and return nonzero;
  - publisher failure is returned;
  - no `git clean` command is issued.
Also make a remote update that conflicts with a preserved untracked file and assert that the pull fails, publication is skipped, and the untracked file remains.

- [ ] **Step 2: Run the job tests to verify the expected failures**

Run: `Invoke-Pester tests\scripts\Job.Tests.ps1`

Expected: tests fail because `job.cmd` does not exist.

- [ ] **Step 3: Implement the root `job.cmd`**

Anchor all paths on `%~dp0`. Require the active branch to be `master` and exit without mutation otherwise. Run `git reset --hard` to discard tracked local changes, then `git pull --ff-only origin master`, then `call` the root `publish.bat`. Check each `errorlevel`, stop at the first failure, and never call `git clean`.

- [ ] **Step 4: Run the job tests and the complete verification set**

Run: `Invoke-Pester tests\scripts\Job.Tests.ps1`

Expected: all isolated job sequencing, path, and failure-propagation tests pass.

Run: `Invoke-Pester tests\scripts`

Expected: all script tests pass without touching the real checkout or IIS publish directory.

Run: `dotnet test tests\PathfinderDb.Modern.Tests\PathfinderDb.Modern.Tests.csproj`

Expected: all modern unit tests pass.

Run: `dotnet build PathfinderDb.sln --configuration Release`

Expected: solution build succeeds with zero errors.

Finally inspect the working tree and search current README/instructions/scripts for obsolete active paths (`src\PathfinderDb.Modern`, root legacy solution, or legacy `build.proj`). Remaining references are permitted only in `archive/legacy` and dated historical specs/plans.
