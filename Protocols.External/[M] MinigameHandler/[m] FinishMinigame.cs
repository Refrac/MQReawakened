using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Extensions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Entities.Components.GameObjects.Trigger.Interfaces;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models.Arenas;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using System.Globalization;
using Web.Apps.Leaderboards.Data;
using Web.Apps.Leaderboards.Database.Scores;
using Web.Apps.Leaderboards.Enums;

namespace Protocols.External._M__MinigameHandler;

public class FinishedMinigame : ExternalProtocol
{
    public override string ProtocolName => "Mm";

    public WorldStatistics WorldStatistics { get; set; }
    public InternalLoot LootCatalog { get; set; }
    public ILogger<FinishedMinigame> Logger { get; set; }
    public TopScoresHandler TopScoresHandler { get; set; }
    public InternalLeaderboards Leaderboards { get; set; }

    public override void Run(string[] message)
    {
        var arenaObjectId = message[5];
        var finishedRaceTime = float.Parse(message[6]);

        var bestTime = finishedRaceTime * 1000;
        var leaderboardTime = finishedRaceTime * 100;

        Logger.LogInformation("Minigame with ID ({minigameId}) has completed.", arenaObjectId);

        var players = Player.Room.GetPlayers();

        foreach (var player in players)
            player.SendXt("Mt", arenaObjectId, Player.CharacterId, finishedRaceTime);

        if (Player.Character.BestMinigameTimes.TryGetValue(Player.Room.LevelInfo.Name, out var time))
        {
            if (bestTime < time)
                Player.Character.BestMinigameTimes[Player.Room.LevelInfo.Name] = bestTime;
        }
        else
        {
            Player.Character.BestMinigameTimes.TryAdd(Player.Room.LevelInfo.Name, bestTime);
        }

        var trigger = Player.Room.GetEntityFromId<ITriggerComp>(arenaObjectId);

        if (trigger == null)
        {
            Logger.LogError("Cannot find statue with ID: {ID}", arenaObjectId);
            return;
        }

        trigger.RemovePhysicalInteractor(Player, Player.GameObjectId);

        if (trigger.GetPhysicalInteractorCount() <= 0)
        {
            foreach (var player in players)
                FinishMinigame(player, arenaObjectId, players.Length);

            trigger.RunTrigger(Player);
            trigger.ResetTrigger();
        }

        var game = Leaderboards.Games.FirstOrDefault(x => x.name == Player.Room.LevelInfo.Name);
        
        if (game == null)
            return;

        var scoreTime = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'sszzz");

        var score = new TopScore
        (
            (int)leaderboardTime,
            0,
            scoreTime,
            Player.Character.Id
        );

        var topScores = TopScoresHandler.GetScoresFromId(game.id);

        if (topScores == null)
        {
            var scoreDaily = new TopScore(score, ScoreType.Daily);
            var scoreWeekly = new TopScore(score, ScoreType.Weekly);

            var scores = new List<TopScore> { score, scoreDaily, scoreWeekly };

            TopScoresHandler.Create(game.id, scores);

            Player.SendXt("Ms", Player.Room.LevelInfo.Name);
            return;
        }

        var newHighScore = false;

        if (topScores.Scores.Any(x => x.CharacterId == Player.Character.Id))
        {
            var existingScores = topScores.Scores.FindAll(x => x.CharacterId == Player.Character.Id).DeepCopy();
            var existingTypes = existingScores.Select(x => x.ScoreType).ToHashSet();

            foreach (var existingScore in existingScores)
            {
                var scoreDate = DateTime.Parse(existingScore.Time);

                switch (existingScore.ScoreType)
                {
                    case ScoreType.Daily:
                        if (existingScore.Score > score.Score || scoreDate.Date < DateTime.Now.Date)
                        {
                            topScores.Scores.Remove(existingScore);
                            topScores.Scores.Add(new TopScore(score, ScoreType.Daily));
                            newHighScore = true;
                        }
                        break;
                    case ScoreType.Weekly:
                        if (existingScore.Score > score.Score || ISOWeek.GetWeekOfYear(scoreDate) != ISOWeek.GetWeekOfYear(DateTime.Now) || scoreDate.Year != DateTime.Now.Year)
                        {
                            topScores.Scores.Remove(existingScore);
                            topScores.Scores.Add(new TopScore(score, ScoreType.Weekly));
                            newHighScore = true;
                        }
                        break;
                    default:
                        if (existingScore.Score > score.Score)
                        {
                            topScores.Scores.Remove(existingScore);
                            topScores.Scores.Add(score);
                            newHighScore = true;
                        }
                        break;
                }
            }

            foreach (var scoreType in Enum.GetValues<ScoreType>())
            {
                if (!existingTypes.Contains(scoreType))
                {
                    topScores.Scores.Add(new TopScore(score, scoreType));
                    newHighScore = true;
                }
            }
        }
        else
        {
            topScores.Scores.Add(score);
            topScores.Scores.Add(new TopScore(score, ScoreType.Daily));
            topScores.Scores.Add(new TopScore(score, ScoreType.Weekly));

            newHighScore = true;
        }

        if (newHighScore)
        {
            TopScoresHandler.Update(topScores.Write);

            Player.SendXt("Ms", Player.Room.LevelInfo.Name);
        }
    }

    public void FinishMinigame(Player player, string minigameId, int membersInRoom)
    {
        player.SendSyncEventToPlayer(new TriggerUpdate_SyncEvent(minigameId, player.Room.Time, membersInRoom));

        var bananaReward = WorldStatistics.GetValue(ItemEffectType.BananaReward, WorldStatisticsGroup.Price, player.Character.GlobalLevel);
        var xpReward = (player.Character.ReputationForNextLevel - player.Character.ReputationForCurrentLevel) *
            WorldStatistics.GlobalStats[Globals.MinigameXPMultiplier] + WorldStatistics.GetValue(ItemEffectType.IncreaseExpFromMinigameLT16,
            WorldStatisticsGroup.Player, player.Character.GlobalLevel);

        var lootedItems = ArenaModel.GrantLootedItems(LootCatalog, player.Room.LevelInfo.LevelId, minigameId);
        var lootableItems = ArenaModel.GrantLootableItems(LootCatalog, player.Room.LevelInfo.LevelId, minigameId);

        var sb = new SeparatedStringBuilder('<');

        sb.Append(membersInRoom.ToString());
        sb.Append(bananaReward.ToString());
        sb.Append(xpReward.ToString());
        sb.Append(lootedItems.ToString());
        sb.Append(lootableItems.ToString());

        player.SendXt("Mp", minigameId, sb.ToString());

        player.SendCashUpdate();

        CheckPlacementObjective(player, placement, membersInRoom);
    }
    public class RepData() : ITimerData
    {
        public Player Player;
        public int Reputation;

        public bool IsValid() => Player != null;
    }

    private void AddReputation(ITimerData data)
    {
        if (data is not RepData rep)
            return;

        var player = rep.Player;

        if (!player.Character.CurrentCollectedDailies.TryGetValue("WarriorGames", out var daily))
            daily = AddDailyHarvest(player);

        daily.TimesHarvested++;

        player.AddReputation(rep.Reputation, Config, InternalAchievement, Logger, RwConfig, ItemCatalog);
    }

    private bool CanGetDailyBoost(Player player)
    {
        if (player.Character.CurrentCollectedDailies.TryGetValue("WarriorGames", out var daily))
        {
            Logger.LogInformation("Player {character} has run Warrior Games {runs} times today.", player.CharacterName, daily.TimesHarvested);
            if (DateTime.Now.Date > daily.TimeOfHarvest.Date)
            {
                player.Character.CurrentCollectedDailies.Remove("WarriorGames");
                return true;
            }
            if (daily.TimesHarvested > 9)
                return false;
        }
        return true;
    }

    private DailiesModel AddDailyHarvest(Player player)
    {
        Logger.LogInformation("Adding new Warrior Games daily for player {character}.", player.CharacterName);
        player.Character.CurrentCollectedDailies.TryAdd("WarriorGames", new DailiesModel
        {
            GameObjectId = "WarriorGames",
            TimeOfHarvest = DateTime.Now,
            TimesHarvested = 0,
        }
        );

        player.Character.CurrentCollectedDailies.TryGetValue("WarriorGames", out var daily);
        return daily;
    }

    public int CalculateXpMultiplier(double xp, int placement, int numPlayers)
    {
        if (numPlayers == 2)
        {
            if (placement < 4)
                xp *= 1.1;
        }
        else if (numPlayers == 3)
        {
            if (placement < 3)
                xp *= 1.2;
            else if (placement == 3)
                xp *= 1.1;
        }
        else if (numPlayers == 4)
        {
            if (placement == 2)
                xp *= 1.3;
            else if (placement == 2)
                xp *= 1.2;
            else if (placement == 3)
                xp *= 1.1;
        }

        return (int) xp;
    }

    public void CheckPlacementObjective(Player player, int placement, int numPlayers)
    {
        if (numPlayers < 2)
            return;

        var relative = 4 - numPlayers;
        var place = placement - relative;

        for (var j = numPlayers; j > 1; j-- )
            for (var i = place; i <= j; i++)
                if (j >= i )
                    player.CheckObjective(ObjectiveEnum.MinigameMedal, string.Empty, j + "Players_" + i, 1, QuestCatalog);

    }

    public void CheckMedalObjective(Player player, List<string> medals)
    {
        foreach(var medal in medals)
            player.CheckObjective(ObjectiveEnum.MinigameMedal, string.Empty, medal, 1, QuestCatalog);
    }
}
