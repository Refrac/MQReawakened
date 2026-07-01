using A2m.Server;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._B__PetBattleHandler;
public class RequestAI : ExternalProtocol
{
    public override string ProtocolName => "BO";

    public WorldHandler WorldHandler { get; set; }
    
    public override void Run(string[] message)
    {
        var difficulty = (Difficulty)int.Parse(message[5]);

        Player.TempData.PetBattleModel = new PetBattleModel(null, true, true, [], Player.GetLevelId(), difficulty);
        
        WorldHandler.ChangePlayerRoom(Player, 554);
    }
}
