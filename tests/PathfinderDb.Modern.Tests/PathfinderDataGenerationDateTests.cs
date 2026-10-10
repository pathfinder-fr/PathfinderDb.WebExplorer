using Microsoft.Extensions.Options;
using PathfinderDb.Data.Domain;
using PathfinderDb.Data.Import;
using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class PathfinderDataGenerationDateTests
{
    [Fact]
    public async Task Loader_records_the_latest_export_file_write_date()
    {
        var root = Path.Combine(Path.GetTempPath(), $"pathfinder-generation-date-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var exportFiles = new[]
            {
                ("feats.json", """{"Sources":[],"Feats":[]}""", new DateTime(2026, 10, 8, 10, 0, 0, DateTimeKind.Utc)),
                ("spells.json", """{"Sources":[],"Spells":[]}""", new DateTime(2026, 10, 9, 10, 0, 0, DateTimeKind.Utc)),
                ("monsters.json", """{"Sources":[],"Monsters":[]}""", new DateTime(2026, 10, 10, 10, 0, 0, DateTimeKind.Utc))
            };

            foreach (var (name, content, lastWriteTimeUtc) in exportFiles)
            {
                var path = Path.Combine(root, name);
                await File.WriteAllTextAsync(path, content);
                File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
            }

            var result = await new PathfinderDataLoader(Options.Create(new PathfinderDataOptions
            {
                RootPath = root
            })).LoadAsync();

            Assert.True(result.IsSuccess, string.Join(" | ", result.Validation.Errors));
            var generatedAtProperty = typeof(DataSnapshot).GetProperty("GeneratedAt");
            Assert.NotNull(generatedAtProperty);
            Assert.Equal(
                new DateTimeOffset(exportFiles[^1].Item3),
                Assert.IsType<DateTimeOffset>(generatedAtProperty.GetValue(result.Snapshot)));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
