using Microsoft.Extensions.Logging;
using PetDefines;
using Server.Base.Core.Extensions;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.Bundles;
public class PetAbilities : PetAbilitiesXML, IBundledXml<PetAbilities>
{
    public string BundleName => "PetAbilities";
    public BundlePriority Priority => BundlePriority.Lowest;

    public ILogger<PetAbilities> Logger { get; set; }
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
}
