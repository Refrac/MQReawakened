using A2m.Server;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._B__PetBattleHandler;
public class InviteResponse : ExternalProtocol
{
    public override string ProtocolName => "BB";

    public WorldHandler WorldHandler { get; set; }

    public override void Run(string[] message)
    {
        var characterName = message[5];
        var accepted = message[6] == "1";
        var status = message[7];

        var inviter = Player.PlayerContainer.GetPlayerByName(characterName);

        if (inviter == null)
            return;

        if (accepted && status == "0")
        {
            Player.TempData.PetBattleModel = new PetBattleModel(inviter, false, false, [], Player.GetLevelId(), Difficulty.Medium);
            
            inviter.TempData.PetBattleModel = new PetBattleModel(Player, true, false, [], inviter.GetLevelId(), Difficulty.Medium);
            
            inviter.SendXt("BB", characterName, Player.Character.CharacterName);
            
            WorldHandler.ChangePlayerRoom(inviter, 554, "1");
            WorldHandler.ChangePlayerRoom(Player, 554, "2");
        }
        else
        {
            inviter.SendXt("BD", Player.Character.CharacterName, status);
        }
    }
}
