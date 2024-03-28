using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;

namespace Protocols.External._B__PetBattleHandler;
public class PetBattleInvite : ExternalProtocol
{
    public override string ProtocolName => "Bb";

    public override void Run(string[] message)
    {
        var characterName = message[5];
        var inviter = Player.PlayerContainer.GetPlayerByName(characterName);

        if (inviter == null)
            return;

        inviter.SendXt("Bb", Player.CharacterName);
    }
}
