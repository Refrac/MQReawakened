using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Protocols.External._B__PetBattleHandler;

public class EvolvePet : ExternalProtocol
{
    public override string ProtocolName => "Be";
    
    public PetBattlePets PetBattlePets { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ILogger<EvolvePet> Logger { get; set; }
    public ItemRConfig ItemRConfig { get; set; }
    
    public override void Run(string[] message)
    {
        var itemId = int.Parse(message[5]);

        var pet = PetBattlePets.GetPetEvolutionFamily(itemId).First(x => x.itemId == itemId);
        
        var petItem = ItemCatalog.GetItemFromId(itemId);
        
        if (petItem == null)
            return;
        
        var points = Player.Character.GetBattlePoints(ItemCatalog);
        var pointsItem = ItemCatalog.GetItemFromPrefabName("COL_PetBattlePoints");

        if (pet.nextTierBattlePointsNeeded > points)
        {
            Logger.LogError("[ANTICHEAT] Player \"{Name}\" attempted to level pet with less than required points {Points}/{RequiredPoints}",
                Player.CharacterName, points, pet.nextTierBattlePointsNeeded);
            return;
        }
        
        var nextPet = ItemCatalog.GetItemFromId(pet.nextTierItemId);

        if (nextPet == null)
            return;
        
        Player.RemoveItem(pointsItem, pet.nextTierBattlePointsNeeded, ItemCatalog, ItemRConfig);
        
        Player.RemoveItem(petItem, 1, ItemCatalog, ItemRConfig);
        Player.AddItem(nextPet, 1, ItemCatalog);
        
        Player.SendUpdatedInventory();
    }
}
