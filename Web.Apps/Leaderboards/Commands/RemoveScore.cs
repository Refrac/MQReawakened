using Server.Base.Accounts.Enums;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Database.Characters;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.XMLs.Bundles.Internal;
using Server.Reawakened.XMLs.Data.Commands;
using Web.Apps.Leaderboards.Database.Scores;
using Web.Apps.Leaderboards.Enums;

namespace Web.Apps.Leaderboards.Commands;

public class RemoveScore : SlashCommand
{
    public override string CommandName => "/removescore";
    public override string CommandDescription => "Remove a leaderboard score from a player";

    public override List<ParameterModel> Parameters =>
    [
        new ParameterModel()
        {
            Name = "characterName",
            Description = "The character name",
            Optional = false
        },
        new ParameterModel()
        {
            Name = "minigame",
            Description = "The minigame to remove a score from",
            Optional = false
        },
        new ParameterModel()
        {
            Name = "score",
            Description = "The score type to remove (top/all)",
            Optional = false
        }
    ];

    public override AccessLevel AccessLevel => AccessLevel.Moderator;
    
    public TopScoresHandler TopScoresHandler { get; set; }
    public CharacterHandler CharacterHandler { get; set; }
    public InternalLeaderboards InternalLeaderboards { get; set; }
    public PlayerContainer PlayerContainer { get; set; }
    
    public override void Execute(Player player, string[] args)
    {
        var firstName = args[1];
        var lastName = args[2];
        var targetName = firstName + " " + lastName;

        var target = PlayerContainer.GetPlayerByName(targetName);
        var targetImage = new CharacterDbEntry();

        if (target is null)
        {
            var targetEntity = CharacterHandler.GetCharacterFromName(targetName);
            targetImage = CharacterHandler.Get(targetEntity.Id);
        }
        
        if (targetImage is null)
        {
            Log("Please provide a valid player name.", player);
            return;
        }

        var minigame = args[3];
        var minigameId = 0;

        switch (minigame)
        {
            case "monkeyblast":
                minigame = "LV_CRS_Minigame_MonkeyBlast";
                minigameId = InternalLeaderboards.Games.Find(x => x.name == "LV_CRS_Minigame_MonkeyBlast").id;
                break;
            case "warriorrun":
                minigame = "LV_CRS_MiniRace01";
                minigameId = InternalLeaderboards.Games.Find(x => x.name == "LV_CRS_MiniRace01").id;
                break;
            case "midnightwarriorrun":
                minigame = "LV_CRS_MiniRace02";
                minigameId = InternalLeaderboards.Games.Find(x => x.name == "LV_CRS_MiniRace02").id;
                break;
            case "torchfrenzy":
                minigame = "LV_CRS_MiniTorch01";
                minigameId = InternalLeaderboards.Games.Find(x => x.name == "LV_CRS_MiniTorch01").id;
                break;
            case "midnighttorchfrenzy":
                minigame = "LV_CRS_MiniTorch02";
                minigameId = InternalLeaderboards.Games.Find(x => x.name == "LV_CRS_MiniTorch02").id;
                break;
            case "petmasters":
                minigame = "PetBattleArenaLevel";
                minigameId = InternalLeaderboards.Games.Find(x => x.name == "PetBattleArenaLevel").id;
                break;
        }
        
        var minigameType = TopScoresHandler.GetScoresFromGame(minigameId);

        if (minigameType == null)
        {
            Log("Please provide a valid minigame type name.", player);
            return;
        }

        var targetId = 0;
        
        if (target != null)
        {
            targetId = target.CharacterId;
            target.Character.BestMinigameTimes.Remove(minigame);
        }
        else
        {
            targetId = targetImage.Id;
            targetImage.BestMinigameTimes.Remove(minigame);
            CharacterHandler.Update(targetImage);
        }

        var score = args[4];

        if (score.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            var scores = minigameType.FindAll(x => x.CharacterId == targetId);

            foreach (var existingScore in scores)
            {
                TopScoresHandler.Remove(existingScore.Id);
            }
            
            Log("Removed all scores from " + targetName + ".", player);
        }
        else
        {
            var existingScore = minigameType.Find(x => x.CharacterId == targetId && x.ScoreType == ScoreType.AllTime);
            
            TopScoresHandler.Remove(existingScore.Id);
            
            Log("Removed top score from " + targetName + ".", player);
        }
    }
}
