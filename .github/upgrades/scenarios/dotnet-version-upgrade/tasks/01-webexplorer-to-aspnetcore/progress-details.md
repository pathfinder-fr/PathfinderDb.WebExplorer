# Task 01: Migrate PathfinderDb.WebExplorer to ASP.NET Core on .NET 10 — Progress

## Summary
Converted the single-project ASP.NET MVC/Web API 4 application on .NET Framework 4.8 to an SDK-style ASP.NET Core MVC project targeting `net10.0`, in-place (no side-by-side, no System.Web Adapters).

## Changes

### Project file
- Converted `PathfinderDb.WebExplorer.csproj` to SDK-style (`Microsoft.NET.Sdk.Web`), retargeted to `net10.0`.
- Removed all legacy `System.Web`/ASP.NET 4 package references (`Microsoft.AspNet.Mvc`, `Microsoft.AspNet.WebApi*`, `Microsoft.AspNet.WebPages`, `Microsoft.AspNet.Razor`, `Microsoft.Net.Http`, `Microsoft.Web.Infrastructure`, `Twitter.Bootstrap` package reference — static assets kept as files).
- Kept only `Newtonsoft.Json` (upgraded 4.5.10 → 13.0.4) and `PathfinderDb.Schema` (upgraded 1.0.1 → 1.0.3) as package references.
- `App_Data` XML content files preserved as `Content` items for local/offline data.

### Removed legacy artifacts
- `Global.asax` / `Global.asax.cs`
- `App_Start/RouteConfig.cs`, `App_Start/FilterConfig.cs`, `App_Start/WebApiConfig.cs`
- `Web.config`, `Web.Debug.config`, `Web.Release.config`, `Views/Web.config`, `Raw/Web.config`
- `packages.config`
- `Properties/AssemblyInfo.cs`, `Properties/PublishProfiles/Druadan.pubxml`

### New ASP.NET Core hosting
- Added `Program.cs` with minimal hosting model: `AddControllersWithViews()`, static files, routing, default `{controller=Home}/{action=Index}/{id?}` route, and `MemoryDataSet.Initialize(app.Environment.ContentRootPath)` at startup.
- Added `Views/_ViewImports.cshtml` with `DbBrowser`/`DbBrowser.Models`/`Microsoft.AspNetCore.Routing` usings and the MVC TagHelpers.

### Model/controller updates
- `Models/MemoryDataSet.cs`: replaced `HttpServerUtilityBase.MapPath` with `contentRootPath`-based `Path.Combine`; added `Initialize(string)` entry point used from `Program.cs`.
- `Controllers/ControllerExtensions.cs`: replaced `controller.Server` with `IWebHostEnvironment` resolved from `HttpContext.RequestServices`.
- All controllers (`HomeController`, `FeatController`, `SpellController`) updated from `System.Web.Mvc` to `Microsoft.AspNetCore.Mvc`.
- `FeatIndexQuery.cs` / `SpellIndexQuery.cs`: `System.Web.Routing.RouteValueDictionary` → `Microsoft.AspNetCore.Routing.RouteValueDictionary`.
- `Models/FeatExtensions.cs`: rewrote `RenderPrerequisites` from `HelperResult`/`MvcHtmlString` (System.Web.WebPages) to `IHtmlContent`/`HtmlContentBuilder` (Microsoft.AspNetCore.Html), preserving the same rendered markup.
- Added `Models/HtmlHelperExtensions.cs` with one additional `BeginForm` overload (action, controller, routeValues, method, htmlAttributes) needed by `FeatNav.cshtml` that isn't provided natively by ASP.NET Core's `HtmlHelperFormExtensions`.

### Views
- `Views/Shared/_Layout.cshtml`: replaced `this.ViewContext.Controller.GetType().Name` (not available in ASP.NET Core `ViewContext`) with `this.ViewContext.RouteData.Values["controller"]`.
- Other views (`Feat/*.cshtml`, `Spell/Index.cshtml`, `Home/Index.cshtml`) compiled as-is once `RouteValueDictionary` was imported via `_ViewImports.cshtml` — `Html.ActionLink`/`Html.BeginForm` calls resolved to native ASP.NET Core MVC `HtmlHelperLinkExtensions`/`HtmlHelperFormExtensions` overloads.

## Build Result
`dotnet build` (via run_build tool) — **Build successful**, 0 errors.

## Not yet done / follow-ups
- Manual local run/UI verification against reference site db.pathfinder-fr.org is covered by task 02 (final validation).
- Static assets (Bootstrap CSS/JS, images) are still served as plain files under `Content`/`Scripts`; consider moving to `wwwroot` per ASP.NET Core convention as a future improvement (not required for functional parity, deferred).
