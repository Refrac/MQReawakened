
using Server.Reawakened.Players.Helpers;

namespace Server.Reawakened.Players.Models.Pets;
public class PetProfileModel
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public int TypeId { get; set; }
    public int Energy { get; set; }
    public int FoodToConsume { get; set; }
    public int TimeToConsume { get; set; }
    public int BoostXp { get; set; }

    public override string ToString()
    {
        var sb = new SeparatedStringBuilder('|');

        sb.Append(Id);
        sb.Append(ItemId);
        sb.Append(TypeId);
        sb.Append(Energy);
        sb.Append(FoodToConsume);
        sb.Append(TimeToConsume);
        sb.Append(BoostXp);

        return sb.ToString();
    }
}
