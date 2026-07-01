using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._B__PetBattleHandler;
public class SelectedPets : ExternalProtocol
{
    public override string ProtocolName => "Bs";

    public PetBattlePets PetBattlePets { get; set; }

    public override void Run(string[] message)
    {
        var pets = message[6].Split(",");
        var aiPets = message[7].Split(",");

        var model = Player.TempData.PetBattleModel;
        
        foreach (var pet in pets)
        {
            var battlePet = PetBattlePets.GetPetEvolutionFamily(int.Parse(pet)).FirstOrDefault();

            if (battlePet == null)
                continue;
            
            model.Pets.Add(new PetBattlePetsXML.PetBattlePet
            {
                itemId = battlePet.itemId,
                abilities = battlePet.abilities,
                accuracy = battlePet.accuracy,
                health = battlePet.health,
                id = battlePet.id,
                nextTierItemId = battlePet.nextTierItemId,
                nextTierBattlePointsNeeded = battlePet.nextTierBattlePointsNeeded,
                species = battlePet.species,
                speed = battlePet.speed,
                tier = battlePet.tier
            });
        }

        if (model.IsAI)
            foreach (var pet in aiPets)
            {
                var battlePet = PetBattlePets.GetPetEvolutionFamily(int.Parse(pet)).FirstOrDefault();

                if (battlePet == null)
                    continue;
        
                model.Pets.Add(new PetBattlePetsXML.PetBattlePet
                {
                    itemId = battlePet.itemId,
                    abilities = battlePet.abilities,
                    accuracy = battlePet.accuracy,
                    health = battlePet.health,
                    id = battlePet.id,
                    nextTierItemId = battlePet.nextTierItemId,
                    nextTierBattlePointsNeeded = battlePet.nextTierBattlePointsNeeded,
                    species = battlePet.species,
                    speed = battlePet.speed,
                    tier = battlePet.tier
                });
            }
        
        Player.SendXt("BP", model.IsChallenger ? "1" : "0", 
            model.Pets[0].itemId, model.Pets[1].itemId, model.Pets[2].itemId,
            model.Pets[3].itemId, model.Pets[4].itemId, model.Pets[5].itemId);
    }
}
