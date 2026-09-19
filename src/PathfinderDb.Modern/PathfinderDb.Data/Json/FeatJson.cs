using System.Text.Json;
using System.Text.Json.Serialization;

namespace PathfinderDb.Data.Json;

public sealed class FeatDocument
{
    [JsonPropertyName("Feats")]
    public List<FeatJson>? Feats { get; set; }
    [JsonPropertyName("Sources")]
    public List<SourceJson>? Sources { get; set; }
    [JsonPropertyName("Labels")]
    public List<LabelJson>? Labels { get; set; }
}

public sealed class FeatJson
{
    [JsonPropertyName("Id")]
    public string? Id { get; set; }
    [JsonPropertyName("Name")]
    public string? Name { get; set; }
    [JsonPropertyName("Types")]
    public List<string>? Types { get; set; }
    [JsonPropertyName("Prerequisites")]
    public List<PrerequisiteJson>? Prerequisites { get; set; }
    public string? Description { get; set; }
    public string? Benefit { get; set; }
    public string? Normal { get; set; }
    public SourceJson? Source { get; set; }
    public JsonElement? Localization { get; set; }
}

public sealed class PrerequisiteJson
{
    public string? Type { get; set; }
    public string? OtherType { get; set; }
    public string? Value { get; set; }
    public decimal? Number { get; set; }
    public string? Description { get; set; }
    public List<PrerequisiteJson>? Items { get; set; }
}

public sealed class SourceJson
{
    [JsonPropertyName("Id")]
    public string? Id { get; set; }
    [JsonPropertyName("References")]
    public List<ReferenceJson>? References { get; set; }
}

public sealed class ReferenceJson
{
    public string? Name { get; set; }
    public string? Href { get; set; }
    public string? HrefString { get; set; }
    public string? Lang { get; set; }
}
