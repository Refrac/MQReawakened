using A2m.Server;
using Microsoft.Extensions.Logging;
using PetDefines;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;
public class InternalPetAbilities : IBundledXml<InternalPetAbilities>
{
    public ILogger<InternalPetAbilities> Logger { get; set; }

    public string BundleName => "PetAbilities";

    public BundlePriority Priority => BundlePriority.Lowest;

    public IServiceProvider Services { get; set; }

    public Dictionary<int, PetAbilityParams> AbilityData;

    public void InitializeVariables() => 
        AbilityData = [];

    public void EditDescription(XmlDocument xml)
    {
    }

    public void ReadDescription(string xml)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xml);

        foreach (XmlNode childNode in xmlDocument.ChildNodes)
        {
            if (!(childNode.Name == "PetAbilities")) continue;

            foreach (XmlNode childNode2 in childNode.ChildNodes)
            {
                if (!(childNode2.Name == "Abillity")) continue;

                var petId = -1;
                var petAbilityParams = new PetAbilityParams();

                foreach (XmlAttribute attribute in childNode2.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "prefab_id":
                            petId = BundledXML.ParseInt(attribute.Value);
                            break;
                        case "pet_ability_type_id":
                            petAbilityParams.AbilityType = Enum.Parse<PetAbilityType>(attribute.Value);
                            break;
                        case "elemental":
                            petAbilityParams.Elemental = Enum.Parse<Elemental>(attribute.Value);
                            break;
                        case "cooldownTime":
                            petAbilityParams.CooldownTime = BundledXML.ParseFloat(attribute.Value);
                            break;
                        case "duration":
                            petAbilityParams.Duration = BundledXML.ParseFloat(attribute.Value);
                            break;
                        case "frequency":
                            petAbilityParams.Frequency = BundledXML.ParseFloat(attribute.Value);
                            break;
                        case "initialDelayBeforeUse":
                            petAbilityParams.InitialDelayBeforeUse = BundledXML.ParseFloat(attribute.Value);
                            break;
                        case "itemEffectStatRatio":
                            petAbilityParams.ItemEffectStatRatio = BundledXML.ParseFloat(attribute.Value);
                            break;
                        case "useCount":
                            petAbilityParams.UseCount = BundledXML.ParseInt(attribute.Value);
                            break;
                        case "maxHitCount":
                            petAbilityParams.HitCount = BundledXML.ParseInt(attribute.Value);
                            break;
                        case "damageAreaX":
                            petAbilityParams.DamageArea.x = BundledXML.ParseFloat(attribute.Value);
                            break;
                        case "damageAreaY":
                            petAbilityParams.DamageArea.y = BundledXML.ParseFloat(attribute.Value);
                            break;
                        case "damageAreaOffsetX":
                            petAbilityParams.DamageAreaOffset.x = BundledXML.ParseFloat(attribute.Value);
                            break;
                        case "damageAreaOffsetY":
                            petAbilityParams.DamageAreaOffset.y = BundledXML.ParseFloat(attribute.Value);
                            break;
                        case "detectionZoneX":
                            petAbilityParams.DetectionZone.x = BundledXML.ParseFloat(attribute.Value);
                            break;
                        case "detectionZoneY":
                            petAbilityParams.DetectionZone.y = BundledXML.ParseFloat(attribute.Value);
                            break;
                        case "detectionZoneOffsetX":
                            petAbilityParams.DetectionZoneOffset.x = BundledXML.ParseFloat(attribute.Value);
                            break;
                        case "detectionZoneOffsetY":
                            petAbilityParams.DetectionZoneOffset.y = BundledXML.ParseFloat(attribute.Value);
                            break;
                        case "applyOnHealthRatio":
                            petAbilityParams.ApplyOnHealthRatio = BundledXML.ParseFloat(attribute.Value);
                            break;
                        case "defensiveBonusRatio":
                            petAbilityParams.DefensiveBonusRatio = BundledXML.ParseFloat(attribute.Value);
                            break;
                    }
                }

                AbilityData.TryAdd(petId, petAbilityParams);
            }
        }
    }

    public void FinalizeBundle()
    {
    }

    public PetAbilityParams GetAbilityData(int prefabID)
    {
        if (!AbilityData.TryGetValue(prefabID, out var value))
        {
            Logger.LogWarning("Pet with id {prefabId} does not exist in AbilityData", prefabID);
            return null;
        }

        return value;
    }
}
