using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._B__PetBattleHandler;
public class PetBattleInviteResponse : ExternalProtocol
{
    public override string ProtocolName => "BB";

    public WorldHandler WorldHandler { get; set; }
    public ILogger<PetBattleInviteResponse> Logger { get; set; }

    public override void Run(string[] message)
    {
        var characterName = message[5];
        var status = message[6] == "1";

        var inviter = Player.PlayerContainer.GetPlayerByName(characterName);

        if (inviter == null)
            return;

        if (status)
        {
            inviter.SendXt("BB", characterName, Player.CharacterName);

            ChangeLevel(inviter);
        }
        else
            inviter.SendXt("BD", Player.CharacterName, status ? "1" : "0");
    }

    private void ChangeLevel(Player inviter)
    {
        Player.Character.SetLevel(554, Logger);
        inviter.Character.SetLevel(554, Logger);

        Player.SendLevelChange(WorldHandler);
        inviter.SendLevelChange(WorldHandler);
    }
}
