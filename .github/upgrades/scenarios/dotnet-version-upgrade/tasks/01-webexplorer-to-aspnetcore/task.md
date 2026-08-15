# 01-webexplorer-to-aspnetcore: Migrate PathfinderDb.WebExplorer to ASP.NET Core on .NET 10

Convert `PathfinderDb.WebExplorer.csproj` from the legacy (non SDK-style) Web Application Project format to SDK-style, retarget to `net10.0`, and rewrite the ASP.NET MVC/WebAPI 4 application in-place as ASP.NET Core MVC. This is a single project with 60 files; per the confirmed upgrade options this is an in-place rewrite (not side-by-side) using direct migration to native ASP.NET Core APIs (no System.Web Adapters compatibility shim).

Key areas of work, based on assessment findings:
- Convert `Global.asax.cs` initialization (route registration, filter registration, application start logic) into `Program.cs` minimal hosting model
- Replace `RouteCollection`-based route registration with ASP.NET Core endpoint route mapping
- Replace `GlobalFilterCollection` with equivalent ASP.NET Core MVC filter/middleware registration
- Migrate controllers and Razor views from ASP.NET MVC 4 / Web API 4 to ASP.NET Core MVC conventions; replace `HttpContext.Current` and other `System.Web` API usage directly with ASP.NET Core equivalents
- Remove incompatible/superseded packages: `Microsoft.AspNet.Mvc`, `Microsoft.AspNet.Razor`, `Microsoft.AspNet.WebApi`, `Microsoft.AspNet.WebApi.Client`, `Microsoft.AspNet.WebApi.Core`, `Microsoft.AspNet.WebApi.WebHost`, `Microsoft.AspNet.WebPages`, `Microsoft.Net.Http`, `Microsoft.Web.Infrastructure` — this functionality is provided by the ASP.NET Core framework reference
- Verify/update `PathfinderDb.Schema` (currently flagged incompatible) for `net10.0` compatibility
- Upgrade `Newtonsoft.Json` from 4.5.10 to 13.0.4 (addresses a known security vulnerability)
- Keep `Twitter.Bootstrap` (already compatible) and adapt static asset/bundling setup to ASP.NET Core conventions if needed

**Done when**: The project file is SDK-style targeting `net10.0`; the solution builds with 0 errors and 0 warnings; the application starts and serves its main pages/routes correctly when run locally; no `System.Web` references remain; all incompatible packages have been removed or replaced.
