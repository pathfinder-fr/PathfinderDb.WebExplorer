var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<PathfinderDb.Data.Import.PathfinderDataOptions>(
    builder.Configuration.GetSection("PathfinderData"));
builder.Services.AddSingleton<PathfinderDb.Data.Import.PathfinderDataLoader>();
builder.Services.AddSingleton<PathfinderDb.Data.Runtime.IDataSnapshotProvider, PathfinderDb.Data.Runtime.DataSnapshotProvider>();
builder.Services.AddSingleton<PathfinderDb.Data.Domain.CatalogService>();
builder.Services.AddSingleton<PathfinderDb.Web.IGitRepository, PathfinderDb.Web.GitRepository>();
builder.Services.AddHostedService<PathfinderDb.Web.DataInitializationService>();
builder.Services.AddHostedService<PathfinderDb.Web.DataRefreshService>();
builder.Services.AddRazorPages();

var app = builder.Build();
app.MapRazorPages();
app.Run();

public partial class Program;
