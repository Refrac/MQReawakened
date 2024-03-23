using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;

namespace Protocols.External._B__PetBattleHandler;
public class RequestAI : ExternalProtocol
{
    public override string ProtocolName => "BO";

    public WorldHandler WorldHandler { get; set; }
    public ILogger<RequestAI> Logger { get; set; }
    
    public override void Run(string[] message)
    {
        var difficulty = (Difficulty)int.Parse(message[5]);

        Player.SendXt("BO", difficulty);

        Player.TempData.PetBattleModel = new PetBattleModel(Player, true, true, [], Player.GetLevelId());

        ChangeLevel();
    }

    private void ChangeLevel()
    {
        Player.Character.SetLevel(554, Logger);

        Player.SendLevelChange(WorldHandler);
    }
}
