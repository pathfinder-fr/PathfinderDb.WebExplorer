using System.Text.Json;
using Microsoft.Extensions.Options;
using PathfinderDb.Data.Domain;
using PathfinderDb.Data.Import;
using PathfinderDb.Data.Json;
using PathfinderDb.Data.Runtime;
using PathfinderDb.Web;
using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class LocalizedLabelTests
{
    [Fact]
    public void Resolves_french_labels_from_the_generated_catalog_with_key_fallback()
    {
        var document = JsonSerializer.Deserialize(
            """
            {
              "Labels": [
                {
                  "Domain": "spellSchool",
                  "Key": "conjuration",
                  "Translations": [
                    { "Language": "fr-FR", "Value": "Invocation" }
                  ]
                }
              ]
            }
            """,
            PathfinderJsonContext.Default.LabelDocument);

        var catalog = new LabelCatalog(document!.Labels!);

        Assert.Equal("Invocation", catalog.Get("spellSchool", "conjuration"));
        Assert.Equal("unknown", catalog.Get("spellSchool", "unknown"));
    }

    [Fact]
    public async Task Loader_publishes_labels_from_the_shared_catalog_file()
    {
        var root = Path.Combine(Path.GetTempPath(), $"pathfinder-labels-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "feats.json"), """{"Sources":[],"Feats":[]}""");
            await File.WriteAllTextAsync(Path.Combine(root, "spells.json"), """{"Sources":[],"Spells":[]}""");
            await File.WriteAllTextAsync(Path.Combine(root, "monsters.json"), """{"Sources":[],"Monsters":[]}""");
            await File.WriteAllTextAsync(Path.Combine(root, "labels.json"),
                """{"Labels":[{"Domain":"spellSchool","Key":"conjuration","Translations":[{"Language":"fr-FR","Value":"Invocation"}]}]}""");

            var result = await new PathfinderDataLoader(Options.Create(new PathfinderDataOptions
            {
                RootPath = root
            })).LoadAsync();

            Assert.True(result.IsSuccess, string.Join(" | ", result.Validation.Errors));
            Assert.Equal("Invocation", result.Snapshot!.Labels.Get("spellSchool", "conjuration"));
            Assert.Equal("Fée", new LabelCatalog(
                [
                    new LabelJson
                    {
                        Domain = "creatureType",
                        Key = "fey",
                        Translations = [new LabelTranslationJson { Language = "fr-FR", Value = "Fée" }]
                    }
                ]).Get("creatureType", "Fey"));
            Assert.Equal("Invocation", new CatalogText(new FixedSnapshotProvider(result.Snapshot))
                .FormatSpellSchool("conjuration"));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private sealed class FixedSnapshotProvider(DataSnapshot snapshot) : IDataSnapshotProvider
    {
        public DataSnapshot? Current => snapshot;
        public DataLoadStatus Status => DataLoadStatus.ReadyStatus(snapshot, []);
        public Task<DataLoadStatus> LoadInitialAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Status);
        public Task<DataLoadStatus> ReloadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Status);
    }
}
