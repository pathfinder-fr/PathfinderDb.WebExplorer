using System.Text.Json;
using PathfinderDb.Data.Json;
using Xunit;

namespace PathfinderDb.Modern.Tests;

public sealed class JsonContractTests
{
    [Fact]
    public void Contracts_preserve_heterogeneous_prerequisites_and_dynamic_spell_lists()
    {
        var feat = JsonSerializer.Deserialize(
            """{"Feats":[{"Id":"fleet","Name":"Fleet","Prerequisites":[{"OtherType":"ExoticWeaponProficiency","Value":"filet"},{"Items":[{"Type":"Feat","Value":"run"}]}]}],"Sources":[{"Id":"uc"}]}""",
            PathfinderJsonContext.Default.FeatDocument);
        var spell = JsonSerializer.Deserialize(
            """{"Spells":[{"Id":"ray","Name":"Ray","Levels":[{"List":"psychiste","Level":1}],"Unknown":true}],"Sources":[]}""",
            PathfinderJsonContext.Default.SpellDocument);

        Assert.Equal("ExoticWeaponProficiency", feat!.Feats![0].Prerequisites![0].OtherType);
        Assert.Equal("psychiste", spell!.Spells![0].Levels![0].List);
    }
}
