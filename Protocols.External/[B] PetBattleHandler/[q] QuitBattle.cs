using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._B__PetBattleHandler;
public class QuitBattle : ExternalProtocol
{
    public override string ProtocolName => "Bq";

    public PlayerContainer PlayerContainer { get; set; }
    public WorldHandler WorldHandler { get; set; }

    public override void Run(string[] message)
    {
        var characterName = message[5];

        foreach (var player in PlayerContainer.GetAllPlayers())
        {
            var battleModel = player.TempData.PetBattleModel;

            WorldHandler.ChangePlayerRoom(player, battleModel.LevelId);
        }

        SendXt("BQ", characterName);
    }
}
