using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Models.Pets;

namespace Protocols.External._B__PetBattleHandler;
public class QuitBattle : ExternalProtocol
{
    public override string ProtocolName => "Bq";

    public override void Run(string[] message)
    {
        var characterName = message[5];

        SendXt("BQ", characterName);
    }
}
