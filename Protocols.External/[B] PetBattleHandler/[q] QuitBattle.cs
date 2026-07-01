using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Protocols.External._B__PetBattleHandler;
public class QuitBattle : ExternalProtocol
{
    public override string ProtocolName => "Bq";

    public WorldStatistics WorldStatistics { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<PetBattleModel> Logger { get; set; }
    public ServerRConfig ServerConfig { get; set; }
    
    public override void Run(string[] message)
    {
        var characterName = message[5];

        var model = Player.TempData.PetBattleModel;

        if (!model.IsAI)
        {
            var enemyModel = model.Enemy.TempData.PetBattleModel;
            
            var bananas = enemyModel.GetBananas(model.Enemy, WorldStatistics, InternalAchievement, Logger);
            var xp = enemyModel.GetXp(model.Enemy, WorldStatistics, ServerConfig);
            var battlePoints = enemyModel.GetBattlePoints(model.Enemy, true, ItemCatalog);
            
            enemyModel.BattleOver = true;
            
            model.Enemy.SendXt("BC", 1, 1, 0, 0, 0, 0, bananas, xp, battlePoints, 0, 0);
        }
        
        model.BattleOver = true;
        
        Player.SendXt("BC", 0, 0, 1, 1, 0, 0, 0, 0, 0, 0);
    }
}
