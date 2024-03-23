using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.XMLs.Bundles;

namespace Protocols.External._B__PetBattleHandler;
public class SelectedPets : ExternalProtocol
{
    public override string ProtocolName => "Bs";

    public PetBattlePets PetBattlePets { get; set; }

    public override void Run(string[] message)
    {
        var pets = message[6].Split(",");
        var enemyPets = message[7].Split(",");

        if (pets == null || enemyPets == null)
            return;

        var model = Player.TempData.PetBattleModel;

        if (model == null)
            return;

        foreach (var pet in pets)
        {
            var battlePet = PetBattlePets.GetPetEvolutionFamily(int.Parse(pet)).FirstOrDefault(x => x.itemId == int.Parse(pet));

            model.Pets.Add(battlePet);
        }

        foreach (var pet in enemyPets)
        {
            var battlePet = PetBattlePets.GetPetEvolutionFamily(int.Parse(pet)).FirstOrDefault(x => x.itemId == int.Parse(pet));

            model.Pets.Add(battlePet);
        }

        Player.SendXt("BP", model.IsChallenger ? "1" : "0", model.Pets[0].itemId, model.Pets[1].itemId, model.Pets[2].itemId, model.Pets[3].itemId, model.Pets[4].itemId, model.Pets[5].itemId);
    }
}
