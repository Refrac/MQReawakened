using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Models.Pets;

namespace Protocols.External._B__PetBattleHandler;
public class InitBattle : ExternalProtocol
{
    public override string ProtocolName => "Bi";

    public override void Run(string[] message)
    {
        var characterName = message[5];

        var model = Player.TempData.PetBattleModel;

        if (model == null)
            Player.TempData.PetBattleModel = new PetBattleModel(Player, true, true, [], Player.Room.LevelInfo.LevelId);

         SendXt("BI", "1", characterName, Player.CharacterName);
    }
}
