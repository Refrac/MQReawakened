using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Server.Reawakened.Players.Models.Pets;
public class PetBattleModel
{
    public Player Enemy { get; set; }
    public bool IsChallenger { get; set; }
    public bool IsAI { get; set; } = false;
    public List<PetBattlePetsXML.PetBattlePet> Pets { get; set; }
    public int LevelId { get; set; }
    public Difficulty Difficulty { get; set; }
    public bool MyTurn { get; set; } = false;
    public bool BattleOver { get; set; } = false;

    public PetBattleModel(Player enemy, bool isChallenger, bool isAI, List<PetBattlePetsXML.PetBattlePet> pets, int levelId, Difficulty difficulty)
    {
        Enemy = enemy;
        IsChallenger = isChallenger;
        IsAI = isAI;
        Pets = pets;
        LevelId = levelId;
        Difficulty = difficulty;
    }

    public int GetBananas(Player player, WorldStatistics worldStatistics, InternalAchievement internalAchievement, ILogger<PetBattleModel> logger)
    {
        var bananaReward = worldStatistics.GetValue(ItemEffectType.BananaReward, WorldStatisticsGroup.Price, player.Character.GlobalLevel);
        
        player.AddBananas(bananaReward, internalAchievement, logger);
        
        return bananaReward;
    }

    public float GetXp(Player player, WorldStatistics worldStatistics, ServerRConfig rConfig)
    {
        var xpReward = (player.Character.ReputationForNextLevel - player.Character.ReputationForCurrentLevel) *
            worldStatistics.GlobalStats[Globals.MinigameXPMultiplier] + worldStatistics.GetValue(ItemEffectType.IncreaseExpFromMinigameLT16,
                WorldStatisticsGroup.Player, player.Character.GlobalLevel);
        
        player.AddReputation((int)xpReward, rConfig);
        
        return xpReward;
    }

    public int GetBattlePoints(Player player, bool winner, ItemCatalog itemCatalog)
    {
        var battlePoints = 80 * (int)Difficulty;

        if (winner)
            battlePoints *= 2;

        var pointsItem = itemCatalog.GetItemFromPrefabName("COL_PetBattlePoints");
        
        if (pointsItem != null)
            player.AddItem(pointsItem, battlePoints, itemCatalog);
        
        return battlePoints;
    }
}
