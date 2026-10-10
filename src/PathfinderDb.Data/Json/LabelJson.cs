using System.Text.Json.Serialization;

namespace PathfinderDb.Data.Json;

public sealed class LabelDocument
{
    [JsonPropertyName("Labels")]
    public List<LabelJson>? Labels { get; set; }
}

public sealed class LabelJson
{
    public string? Domain { get; set; }
    public string? Key { get; set; }
    public List<LabelTranslationJson>? Translations { get; set; }
}

public sealed class LabelTranslationJson
{
    public string? Language { get; set; }
    public string? Value { get; set; }
}
