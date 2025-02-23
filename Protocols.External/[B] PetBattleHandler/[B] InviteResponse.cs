using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.Rooms.Extensions;
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
            Player.TempData.PetBattleModel = new PetBattleModel(Player, false, false, [], Player.GetLevelId());

            inviter.TempData.PetBattleModel = new PetBattleModel(inviter, true, false, [], inviter.GetLevelId());

            inviter.SendXt("BB", characterName, Player.CharacterName);

            WorldHandler.ChangePlayerRoom(Player, 554, "0");
            WorldHandler.ChangePlayerRoom(inviter, 554, "1");
        }
        else
            inviter.SendXt("BD", Player.CharacterName, status ? "1" : "0");
    }
}
