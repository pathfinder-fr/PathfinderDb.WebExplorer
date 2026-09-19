using System.Text.Json.Serialization;

namespace PathfinderDb.Data.Json;

public sealed class MonsterDocument
{
    [JsonPropertyName("Monsters")]
    public List<MonsterJson>? Monsters { get; set; }
    [JsonPropertyName("Sources")]
    public List<SourceJson>? Sources { get; set; }
    [JsonPropertyName("Labels")]
    public List<LabelJson>? Labels { get; set; }
}

public sealed class MonsterJson
{
    [JsonPropertyName("Id")]
    public string? Id { get; set; }
    [JsonPropertyName("Name")]
    public string? Name { get; set; }
    public decimal? CR { get; set; }
    public string? Climate { get; set; }
    public string? Environment { get; set; }
    public string? Type { get; set; }
    public SourceJson? Source { get; set; }
}
