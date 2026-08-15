# Upgrade Options — PathfinderDb.WebExplorer

Assessment: 1 project (`PathfinderDb.WebExplorer`), ASP.NET MVC/WebAPI on .NET Framework 4.8, legacy (non SDK-style) project file, 60 files, 100 issues (90 mandatory).

## Strategy

### Upgrade Strategy
Single project in the solution — no dependency graph to manage.

| Value | Description |
|-------|-------------|
| **All-at-Once** (selected) | Upgrade the single project in one atomic pass: convert to SDK-style, retarget to net10.0, migrate ASP.NET MVC/WebAPI to ASP.NET Core, fix packages. |

## Project Structure

### Project Approach
Small web project (60 files, well under the 10-controller/10k LOC threshold) with no continuous-deployment constraint mentioned — an in-place rewrite is the fastest path.

| Value | Description |
|-------|-------------|
| **In-place rewrite** (selected) | Replace the ASP.NET Framework MVC/WebAPI project entirely with ASP.NET Core MVC in one pass: new `Program.cs`, route mapping, converted controllers/views. |
| Side-by-side | Scaffold a new ASP.NET Core project next to the old one and migrate incrementally behind a reverse proxy. Adds overhead not justified for a project this size. |

## Compatibility

### System.Web Adapters
ASP.NET MVC/WebAPI project references `System.Web`; project is small and using in-place rewrite, so a direct migration to native ASP.NET Core APIs is preferred over adding a compatibility shim layer.

| Value | Description |
|-------|-------------|
| **Direct Migration to ASP.NET Core APIs** (selected) | Replace `HttpContext.Current`, `System.Web` routing, and MVC filters directly with ASP.NET Core equivalents. No compatibility shims to remove later. |
| Use System.Web Adapters | Add `Microsoft.AspNetCore.SystemWebAdapters` for incremental compatibility. Unnecessary overhead for an in-place rewrite of this size. |

### Unsupported Packages
4 packages have no compatible version for net10.0: `Microsoft.AspNet.WebApi.Client`, `Microsoft.AspNet.WebApi.Core`, `Microsoft.AspNet.WebApi.WebHost`, `PathfinderDb.Schema`. Small enough count to resolve within the same task — these are mostly ASP.NET Framework packages being replaced by built-in ASP.NET Core equivalents, plus one internal package (`PathfinderDb.Schema`) that needs a compatibility check.

| Value | Description |
|-------|-------------|
| **Resolve Inline** (selected) | Remove incompatible ASP.NET Framework WebAPI packages (superseded by ASP.NET Core MVC), and verify/update `PathfinderDb.Schema` for net10.0 compatibility within the same task. |
| Defer Resolution | Stub out incompatible packages and create follow-up tasks. Unnecessary — count is small and resolution is straightforward for this project. |
