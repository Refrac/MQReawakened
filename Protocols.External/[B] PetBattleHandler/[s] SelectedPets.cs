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
        var pets = message[6].Split(',');
        var enemyPets = message[7].Split(',');

        if (pets == null || enemyPets == null)
            return;

        var petId = pets[0];
        var petId2 = pets[1];
        var petId3 = pets[2];

        var enemyPetId = enemyPets[0];
        var enemyPetId2 = enemyPets[1];
        var enemyPetId3 = enemyPets[2];

        SendXt("BP", "0", petId, petId2, petId3, enemyPetId, enemyPetId2, enemyPetId3);
    }
}
