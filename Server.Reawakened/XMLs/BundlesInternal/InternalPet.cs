using Microsoft.Extensions.Logging;
using Server.Reawakened.Configs;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.XMLs.Abstractions;
using Server.Reawakened.XMLs.Enums;
using System.Xml;

namespace Server.Reawakened.XMLs.BundlesInternal;
public class InternalPet : InternalXml
{
    public override string BundleName => "InternalPet";
    public override BundlePriority Priority => BundlePriority.Low;

    public ILogger<InternalPet> Logger { get; set; }
    public ServerRConfig Config { get; set; }

    public Dictionary<int, PetProfileModel> PetsDictionary;

    public override void InitializeVariables() => PetsDictionary = [];

    public override void ReadDescription(XmlDocument xmlDocument)
    {
        foreach (XmlNode petXml in xmlDocument.ChildNodes)
        {
            if (petXml.Name != "Pets") continue;

            foreach (XmlNode pet in petXml.ChildNodes)
            {
                if (pet.Name != "Pet") continue;

                var id = -1;
                var itemId = -1;
                var typeId = -1;
                var energy = -1;
                var experience = -1;
                var foodToConsume = -1;
                var timeToConsume = -1L;
                var boostXp = -1;

                foreach (XmlAttribute attribute in pet.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case "id":
                            id = int.Parse(attribute.Value);
                            break;
                        case "itemId":
                            itemId = int.Parse(attribute.Value);
                            break;
                        case "typeId":
                            typeId = int.Parse(attribute.Value);
                            break;
                        case "energy":
                            energy = int.Parse(attribute.Value);
                            break;
                        case "experience":
                            experience = int.Parse(attribute.Value);
                            break;
                        case "foodToConsume":
                            foodToConsume = int.Parse(attribute.Value);
                            break;
                        case "timeToConsume":
                            timeToConsume = long.Parse(attribute.Value);
                            break;
                        case "boostXp":
                            boostXp = int.Parse(attribute.Value);
                            break;
                    }
                }

                var model = new PetProfileModel()
                {
                    Id = id,
                    ItemId = itemId,
                    TypeId = typeId,
                    Energy = energy,
                    FoodToConsume = foodToConsume,
                    Experience = experience,
                    TimeToConsume = timeToConsume,
                    BoostXp = boostXp,
                    GameVersion = Config.GameVersion
                };

                PetsDictionary.TryAdd(itemId, model);
            }
        }
    }

    public PetProfileModel GetPetProfile(int itemId)
    {
        if (!PetsDictionary.TryGetValue(itemId, out var value))
        {
            Logger.LogWarning("Item with id {ItemId} does not exist in the PetsDictionary", itemId);
            return null;
        }

        return value;
    }

    public string GetPets()
    {
        var sb = new SeparatedStringBuilder('>');

        foreach (var pet in PetsDictionary.Values)
            sb.Append(pet);

        return sb.ToString();
    }
}
