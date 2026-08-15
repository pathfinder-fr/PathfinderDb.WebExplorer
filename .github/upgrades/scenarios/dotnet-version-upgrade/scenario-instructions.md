# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0

## Source Control
- **Source Branch**: master
- **Working Branch**: upgrade-dotnet-10
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## Upgrade Options
**Source**: .github/upgrades/scenarios/dotnet-version-upgrade/upgrade-options.md

### Strategy
- Upgrade Strategy: All-at-Once

### Project Structure
- Project Approach: In-place rewrite

### Compatibility
- System.Web Adapters: Direct Migration to ASP.NET Core APIs
- Unsupported Packages: Resolve Inline

## Strategy
**Selected**: All-at-Once
**Rationale**: Single project in the solution, no dependency graph to manage.

### Execution Constraints
- Single atomic upgrade — project file conversion, retarget, and framework migration happen together
- Build and fix all compilation errors in one bounded pass (not an iterative retry loop)
- Validate full solution build succeeds before moving to final validation
- In-place rewrite (no side-by-side scaffold) — direct migration to native ASP.NET Core APIs, no System.Web Adapters shim

## Notes
- The application is a local/offline website explorer (no external dependencies required for data). The live site is hosted at db.pathfinder-fr.org for reference only.
- The site can be run locally as needed to validate behavior during the upgrade.
