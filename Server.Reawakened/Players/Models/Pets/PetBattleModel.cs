namespace Server.Reawakened.Players.Models.Pets;
public class PetBattleModel
{
    public Player Player { get; set; }
    public bool IsChallenger { get; set; }
    public bool IsAI { get; set; }
    public List<PetBattlePetsXML.PetBattlePet> Pets { get; set; }
    public int LevelId { get; set; }

    public PetBattleModel(Player player, bool isChallenger, bool isAI, List<PetBattlePetsXML.PetBattlePet> pets, int levelId)
    {
        Player = player;
        IsChallenger = isChallenger;
        IsAI = isAI;
        Pets = pets;
        LevelId = levelId;
    }
}
