using System.Text.Json;
using System.Text.Json.Serialization;

namespace PathfinderDb.Data.Json;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip)]
[JsonSerializable(typeof(FeatDocument))]
[JsonSerializable(typeof(SpellDocument))]
[JsonSerializable(typeof(MonsterDocument))]
[JsonSerializable(typeof(LabelDocument))]
public partial class PathfinderJsonContext : JsonSerializerContext
{
}
