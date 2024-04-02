using Microsoft.Extensions.Logging;
using PetDefines;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Reflection;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;
public class PetAbilities : PetAbilitiesXML, IBundledXml
{
    public string BundleName => "PetAbilities";
    public BundlePriority Priority => BundlePriority.Medium;

    public ILogger<PetAbilities> Logger { get; set; }

    public Dictionary<int, PetAbilityParams> AbilityData;

    public void InitializeVariables()
    {
        this.SetField<PetAbilitiesXML>("_petAbilityData", new Dictionary<int, PetAbilityParams>());

        AbilityData = [];
    }

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml) =>
        ReadDescriptionXml(xml);

    public void FinalizeBundle()
    {
        var field = typeof(GameFlow).GetField("_petAbilitiesXML",
                    BindingFlags.Static |
                    BindingFlags.NonPublic);

        field.SetValue(null, this);

        AbilityData = (Dictionary<int, PetAbilityParams>)this.GetField<PetAbilitiesXML>("_petAbilityData");
    }

    public new PetAbilityParams GetAbilityData(int itemId)
    {
        if (!AbilityData.TryGetValue(itemId, out var value))
        {
            Logger.LogWarning("Pet with id {itemId} does not exist in AbilityData", itemId);
            return null;
        }

        return value;
    }
}
