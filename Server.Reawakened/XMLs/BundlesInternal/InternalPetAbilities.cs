using A2m.Server;
using Microsoft.Extensions.Logging;
using PetDefines;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;
public class InternalPetAbilities : PetAbilitiesXML, IBundledXml<InternalPetAbilities>
{
    public ILogger<InternalPetAbilities> Logger { get; set; }

    public string BundleName => "PetAbilities";

    public BundlePriority Priority => BundlePriority.Lowest;

    public IServiceProvider Services { get; set; }

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

    public void FinalizeBundle() => 
        AbilityData = (Dictionary<int, PetAbilityParams>)this.GetField<PetAbilitiesXML>("_petAbilityData");

    public new PetAbilityParams GetAbilityData(int prefabID)
    {
        if (!AbilityData.TryGetValue(prefabID, out var value))
        {
            Logger.LogWarning("Pet with id {prefabId} does not exist in AbilityData", prefabID);
            return null;
        }

        return value;
    }
}
