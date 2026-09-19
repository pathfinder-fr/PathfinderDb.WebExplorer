using System.Text.Json;
using System.Text.Json.Serialization;

namespace PathfinderDb.Data.Json;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip)]
[JsonSerializable(typeof(FeatDocument))]
[JsonSerializable(typeof(SpellDocument))]
[JsonSerializable(typeof(MonsterDocument))]
public partial class PathfinderJsonContext : JsonSerializerContext
{
}
