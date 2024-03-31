
using Server.Reawakened.Configs;
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Pets;
public class PetProfileModel
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public int TypeId { get; set; }
    public int Energy { get; set; }
    public int Experience { get; set; }
    public int FoodToConsume { get; set; }
    public int TimeToConsume { get; set; }
    public int BoostXp { get; set; }
    public GameVersion GameVersion = GameVersion.Unknown;

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');

        sb.Append(Id);
        sb.Append(ItemId);
        sb.Append(TypeId);
        sb.Append(Energy);

        if (GameVersion < GameVersion.vLate2012)
            sb.Append(Experience);

        if (GameVersion >= GameVersion.vLate2012)
        {
            sb.Append(FoodToConsume);
            sb.Append(TimeToConsume);
        }

        sb.Append(BoostXp);

        return sb.ToString();
    }
}
