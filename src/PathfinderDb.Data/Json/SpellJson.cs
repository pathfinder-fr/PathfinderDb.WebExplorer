using System.Text.Json;
using System.Text.Json.Serialization;

namespace PathfinderDb.Data.Json;

public sealed class SpellDocument
{
    [JsonPropertyName("Spells")]
    public List<SpellJson>? Spells { get; set; }
    [JsonPropertyName("Sources")]
    public List<SourceJson>? Sources { get; set; }
    [JsonPropertyName("SpellLists")]
    public List<JsonElement>? SpellLists { get; set; }
    [JsonPropertyName("Labels")]
    public List<LabelJson>? Labels { get; set; }
}

public sealed class SpellJson
{
    [JsonPropertyName("Id")]
    public string? Id { get; set; }
    [JsonPropertyName("Name")]
    public string? Name { get; set; }
    public string? School { get; set; }
    public List<SpellLevelJson>? Levels { get; set; }
    public ComponentsJson? Components { get; set; }
    public ValueJson? Range { get; set; }
    public ValueJson? Target { get; set; }
    public ValueJson? CastingTime { get; set; }
    public SourceJson? Source { get; set; }
    public LocalizationJson? Localization { get; set; }
}

public sealed class SpellLevelJson
{
    [JsonPropertyName("List")]
    public string? List { get; set; }
    [JsonPropertyName("Level")]
    public int? Level { get; set; }
}

public sealed class ComponentsJson
{
    public string? Kinds { get; set; }
}

public sealed class ValueJson
{
    public JsonElement? Value { get; set; }
    public string? SpecificValue { get; set; }
}

public sealed class LocalizationJson
{
    public List<LocalizationLanguageJson>? Languages { get; set; }
}

public sealed class LocalizationLanguageJson
{
    public string? Lang { get; set; }
    public List<LocalizationEntryJson>? Entries { get; set; }
}

public sealed class LocalizationEntryJson
{
    public string? Value { get; set; }
    public string? Href { get; set; }
}
