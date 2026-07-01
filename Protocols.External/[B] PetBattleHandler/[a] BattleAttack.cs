using Microsoft.Extensions.Logging;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Network.Protocols;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;

namespace Protocols.External._B__PetBattleHandler;
public class BattleAttack : ExternalProtocol
{
    public override string ProtocolName => "Ba";
    
    public PetBattlePets PetBattlePets { get; set; }
    public WorldStatistics WorldStatistics { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public InternalAchievement InternalAchievement { get; set; }
    public ILogger<PetBattleModel> Logger { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    
    public override void Run(string[] message)
    {
        var attack = int.Parse(message[6]);
        var pet = int.Parse(message[7]);
        var target = int.Parse(message[8]);
        
        var model = Player.TempData.PetBattleModel;
        
        model.MyTurn = !model.MyTurn;
        
        DecipherAbilityType(attack, pet, target, model);
        
        // Complete game if pets are dead
        if (model.Pets.Skip(3).All(PetIsDead))
        {
            var bananas = model.GetBananas(Player, WorldStatistics, InternalAchievement, Logger);
            var xp = model.GetXp(Player, WorldStatistics, ServerRConfig);
            var battlePoints = model.GetBattlePoints(Player, true, ItemCatalog);
            
            model.BattleOver = true;
            
            Player.SendXt("BC", 1, 1, 0, 0, 0, bananas, xp, battlePoints, 0, 0);
            return;
        }
        
        if (model.Pets.SkipLast(3).All(PetIsDead))
        {
            var bananas = model.GetBananas(Player, WorldStatistics, InternalAchievement, Logger);
            var xp = model.GetXp(Player, WorldStatistics, ServerRConfig);
            var battlePoints = model.GetBattlePoints(Player, false, ItemCatalog);
            
            model.BattleOver = true;
            
            Player.SendXt("BC", 0, 0, 1, 0, 0, bananas, xp, battlePoints, 0, 0);
            return;
        }
        
        Player.SendXt("BT", model.MyTurn ? "0" : "1");
    }

    private void DecipherAbilityType(int attack, int pet, int target, PetBattleModel model)
    {
        var battlePet = model.Pets[pet];
        var ability = battlePet.abilities.FirstOrDefault(x => x.index == attack);
        
        var isAoeAbility = target == -1;
        
        if (!isAoeAbility && (PetIsDead(model.Pets[target]) || PetMissed(pet, target, model)))
            return;
        
        //Attacking.
        if (ability.damageValue > 0 && ability.healthMitigation == 0)
        {
            if (isAoeAbility)
            {
                var isEnemy = pet >= 3;
                
                if (isEnemy)
                    foreach (var enemyPet in model.Pets.SkipLast(3))
                    {
                        target = model.Pets.IndexOf(enemyPet);
                        
                        if (PetIsDead(enemyPet) || PetMissed(pet, target, model))
                            continue;
                        
                        DamagePet(ability, enemyPet, model);
                    }
                else
                    foreach (var enemyPet in model.Pets.Skip(3))
                    {
                        target = model.Pets.IndexOf(enemyPet);
                        
                        if (PetIsDead(enemyPet) || PetMissed(pet, target, model))
                            continue;
                        
                        DamagePet(ability, enemyPet, model);
                    }
            }
            else
            {
                DamagePet(ability, model.Pets[target], model);
            }
        }

        //Healing.
        if (ability.healthValue > 0)
        {
            var targetPet = model.Pets[target];
            
            // Lifesteal or normal heal
            if (ability.damageValue > 0)
            {
                var originalPet = PetBattlePets.GetPetEvolutionFamily(battlePet.itemId).FirstOrDefault();
                
                var healValue = Math.Clamp(ability.healthValue, 0, originalPet.health);
                
                battlePet.health += healValue;
                
                Player.SendXt("BH", pet, healValue, battlePet.health, "0");
            }
            else
            {
                var originalPet = PetBattlePets.GetPetEvolutionFamily(targetPet.itemId).FirstOrDefault();
                
                var healValue = Math.Clamp(ability.healthValue, 0, originalPet.health);
                
                targetPet.health += healValue;
                
                Player.SendXt("BH", target, healValue, targetPet.health, "0");
            }
        }
        
        //Shielding.
        if (ability.healthMitigation > 0)
            ShieldPet(ability, isAoeAbility, pet, target, model);
        
        if (ability.cooldownRounds > 0)
            CooldownPet(isAoeAbility, pet, ability.cooldownRounds, model);
        
        if (ability.durationRounds > 0 && ability.damageValue == 0)
            StunPet(ability, isAoeAbility, pet, target, model);
    }

    private void DamagePet(PetBattlePetsXML.PetBattlePetAbility petAbility, PetBattlePetsXML.PetBattlePet targetPet, PetBattleModel model)
    {
        var damageValue = Math.Clamp(petAbility.damageValue, 0, targetPet.health);

        targetPet.health -= damageValue;
        
        if (targetPet.health < 0)
            targetPet.health = 0;
        
        var targetIndex = model.Pets.IndexOf(targetPet);
        
        Player.SendXt("Bd", targetIndex, damageValue, targetPet.health, petAbility.durationRounds, "0");
        
        if (PetIsDead(targetPet))
            PetKo(targetIndex);
    }

    private void CooldownPet(bool isAoeAbility, int pet, int duration, PetBattleModel model)
    {
        if (isAoeAbility)
            foreach (var enemyPet in model.Pets.SkipLast(3))
            {
                var target = model.Pets.IndexOf(enemyPet);
                
                if (PetIsDead(enemyPet))
                    continue;
                
                Player.SendXt("BL", pet, duration, target);
            }
        else
            Player.SendXt("BL", pet, duration, pet);
    }

    private bool PetIsDead(PetBattlePetsXML.PetBattlePet pet) => pet.health <= 0;

    private void PetKo(int target) => Player.SendXt("Bk", target, "0");

    private void StunPet(PetBattlePetsXML.PetBattlePetAbility petAbility, bool isAoeAbility, int pet, int target, PetBattleModel model)
    {
        if (isAoeAbility)
        {
            var isEnemy = pet >= 3;
            
            if (isEnemy)
                foreach (var enemyPet in model.Pets.SkipLast(3))
                {
                    target = model.Pets.IndexOf(enemyPet);
                    
                    if (PetIsDead(enemyPet) || PetMissed(pet, target, model))
                        continue;
                    
                    Player.SendXt("Bd", target, 0, enemyPet.health, petAbility.durationRounds, "0");
                }
            else
                foreach (var enemyPet in model.Pets.Skip(3))
                {
                    target = model.Pets.IndexOf(enemyPet);
                    
                    if (PetIsDead(enemyPet) || PetMissed(pet, target, model))
                        continue;
                    
                    Player.SendXt("Bd", target, 0, enemyPet.health, petAbility.durationRounds, "0");
                }
        }
        else
        {
            if (petAbility.abilityType == "hide")
                Player.SendXt("Bh", target, petAbility.durationRounds);
            else
                Player.SendXt("Bd", target, 0, model.Pets[target].health, petAbility.durationRounds, "0");
        }
    }

    private void ShieldPet(PetBattlePetsXML.PetBattlePetAbility petAbility, bool isAoeAbility, int pet, int target, PetBattleModel model)
    {
        if (isAoeAbility)
        {
            var isEnemy = pet >= 3;
            
            if (isEnemy)
                foreach (var enemyPet in model.Pets.Skip(3))
                {
                    target = model.Pets.IndexOf(enemyPet);
                    
                    if (PetIsDead(enemyPet) || PetMissed(pet, target, model))
                        continue;
                    
                    Player.SendXt("By", target, petAbility.durationRounds, petAbility.healthMitigation, petAbility.damageValue, pet, "0");
                }
            else
                foreach (var enemyPet in model.Pets.SkipLast(3))
                {
                    target = model.Pets.IndexOf(enemyPet);
                    
                    if (PetIsDead(enemyPet) || PetMissed(pet, target, model))
                        continue;
                    
                    Player.SendXt("By", target, petAbility.durationRounds, petAbility.healthMitigation, petAbility.damageValue, pet, "0");
                }
        }
        else
        {
            Player.SendXt("By", target, petAbility.durationRounds, petAbility.healthMitigation, petAbility.damageValue, pet, "0");
        }
    }

    private bool PetMissed(int pet, int target, PetBattleModel model)
    {
        var randomChance = new Random().Next(100);
        if (randomChance > model.Pets[pet].accuracy)
        {
            Player.SendXt("Bm", pet, target);
            return true;
        }
        return false;
    }
}
