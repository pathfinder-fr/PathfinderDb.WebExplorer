namespace PathfinderDb.Data.Import;

public sealed class PathfinderDataOptions
{
    public string? RootPath { get; set; }
    public bool RefreshEnabled { get; set; }
    public int RefreshIntervalMinutes { get; set; } = 1440;
}
