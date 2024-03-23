using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._B__PetBattleHandler;
public class SkipTurn : ExternalProtocol
{
    public override string ProtocolName => "Bt";

    public override void Run(string[] message)
    {
        var characterName = message[5] == Player.CharacterName;

        Player.SendXt("BT", "0");
    }
}
