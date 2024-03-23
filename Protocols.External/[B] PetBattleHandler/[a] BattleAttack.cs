using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._B__PetBattleHandler;
public class BattleAttack : ExternalProtocol
{
    public override string ProtocolName => "Ba";

    public override void Run(string[] message)
    {
        var attack = int.Parse(message[6]);
        var pet = int.Parse(message[7]);
        var target = int.Parse(message[8]);

        // Likely supposed to do an AOE/Multiple Pet attack or something
        if (target == -1)
        {
            Player.SendXt("BT", "1");
            return;
        }

        var model = Player.TempData.PetBattleModel;

        var battlePet = model.Pets[pet];
        var targetPet = model.Pets[target];

        var ability = battlePet.abilities.FirstOrDefault(x => x.index == attack);

        targetPet.health -= ability.damageValue;

        Player.SendXt("Bd", target, ability.damageValue, targetPet.health, ability.cooldownRounds, "0");

        // Knockout target pet
        if (targetPet.health <= 0)
        {
            model.Pets.Remove(targetPet);
            Player.SendXt("Bk", target, "0");
        }

        Player.SendXt("BT", model.Pets.Count >= 3 ? "1" : "0");
    }
}
