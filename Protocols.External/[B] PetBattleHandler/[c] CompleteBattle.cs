using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._B__PetBattleHandler;

public class CompleteBattle : ExternalProtocol
{
    public override string ProtocolName => "Bc";

    public WorldHandler WorldHandler { get; set; }

    public override void Run(string[] message)
    {
        var characterName = message[5];

        SendXt("BQ", characterName);
        
        var model = Player.TempData.PetBattleModel;

        WorldHandler.ChangePlayerRoom(Player, model.LevelId);

        Player.TempData.PetBattleModel = null;
    }
}
