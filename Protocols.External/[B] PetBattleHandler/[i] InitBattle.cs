using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._B__PetBattleHandler;
public class InitBattle : ExternalProtocol
{
    public override string ProtocolName => "Bi";

    public override void Run(string[] message)
    {
        var characterName = message[5];
        var challenger = Player.PlayerContainer.GetPlayerByName(characterName);

        if (challenger == null)
            return;

        SendXt("BI", "1", characterName, Player.CharacterName);
    }
}
