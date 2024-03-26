using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._B__PetBattleHandler;
public class PreAttack : ExternalProtocol
{
    public override string ProtocolName => "BA";

    public override void Run(string[] message)
    {
        var pet = int.Parse(message[6]);
        var attack = int.Parse(message[7]);
        var target = int.Parse(message[8]);
        var team = bool.Parse(message[9]);

        Player.SendXt("BV", pet, attack, target, team);
    }
}
