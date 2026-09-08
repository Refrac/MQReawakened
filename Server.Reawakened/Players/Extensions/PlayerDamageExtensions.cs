using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Timers;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerDamageExtensions
{
    public class UnderwaterData() : PlayerRoomTimer
    {
        if (data is not StatusEffectData status)
            return;

        if (player == null || !player.TempData.IsPoisoned) return;

        var hazardCollider = player.Room.GetCollidersById(status.HazardId).FirstOrDefault();
        if (hazardCollider is null)
            return;
        
        if (!hazardCollider.CheckCollision(player.GetCollider()))
        {
            if (player.TempData.PoisonEffectTimer != null)
            {
                player.TempData.PoisonEffectTimer.Stop();
                player.TempData.PoisonEffectTimer = null;
            }

            player.TempData.IsPoisoned = false;

            player.StopPoisonEffect();
            return;
        }

        player.ApplyCharacterDamage(status.HazardDamage, ItemEffectType.PoisonDamage,
            status.HazardId, status.InvincibilityDuration);
    }

    public static void ApplyCharacterDamage(this Player player, float damage, ItemEffectType effectType,
        string originId, int duration)
    {
        if (player == null || player.TempData.Invincible || player.Character.CurrentLife <= 0) return;

        if (damage <= 0)
            damage = 1;

        if (player.Character.Pets.TryGetValue(player.GetItemIdOfEquippedPet(), out var pet) && pet.ShieldingPlayer)
            Math.Ceiling(damage *= pet.AbilityParams.DefensiveBonusRatio);

        if (player.Character.StatusEffects.HasEffect(ItemEffectType.IncreaseAllResist))
            Math.Ceiling(damage -= (float)(damage * (player.Character.StatusEffects.GetEffect(ItemEffectType.IncreaseAllResist) / 100.0)));

        player.Character.Write.CurrentLife -= (int)damage;

        if (player.Character.CurrentLife < 0 && !player.TempData.IsKnockedOut)
        {
            player.Character.Write.CurrentLife = 0;
            KnockoutPlayer(player);
        }

        player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId.ToString(), player.Room.Time,
            player.Character.CurrentLife, player.Character.MaxLife, originId));

        if (invincibilityDuration <= 0)
            invincibilityDuration = 1;

        player.TemporaryInvincibility(duration);
    }

    public static void ApplyDamageByPercent(this Player player, double percentage, ItemEffectType effectType,
        string hazardId, int duration)
    {
        var health = (double)player.Character.MaxLife;

        var damage = Convert.ToSingle(Math.Ceiling(health * percentage));

        ApplyCharacterDamage(player, damage, effectType, hazardId, duration);
    }

    public static void KnockoutPlayer(this Player player)
    {
        player.TempData.EnemiesInPetAbilityZone = [];

        player.TempData.IsPoisoned = false;
        player.TempData.IsSlowed = false;
        player.TempData.IsSuperStomping = false;
        player.TempData.UnderwaterTime = 0;
        player.TempData.Invincible = true;
        player.TempData.IsKnockedOut = true;

        if (player.TempData.UnderwaterTimer != null)
        {
            player.TempData.UnderwaterTimer.Stop();
            player.TempData.UnderwaterTimer = null;
        }
    }
}
