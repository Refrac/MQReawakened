using A2m.Server;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Timers;
using Server.Reawakened.XMLs.Bundles.Base;

namespace Server.Reawakened.Players.Extensions;

public static class PlayerStatusEffectExtensions
{
    public class StatusEffectData() : PlayerTimer
    {
        public string HazardId;
        public int HazardDamage;
        public ItemEffectType EffectType;
        public float InitialDamageDelay;
        public float DamageDelay;
        public int DamageCount;
        public int InvincibilityDuration;
        public TimerThread TimerThread;
    }

    public static void SendItemEffectToPlayer(this Player player, ItemEffect itemEffect, string prefabFrom, bool sendFx, bool premium = false)
    {
        if (player == null || itemEffect == null) return;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time, (int)itemEffect.Type,
            itemEffect.Value, itemEffect.Duration, sendFx, prefabFrom, premium));
    }

    public static void ApplySlowEffect(this Player player, string hazardId, int damage) =>
        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
        (int)ItemEffectType.SlowStatusEffect, damage, 1, true, hazardId, false));

    //Doesn't seem to apply fast enough.
    public static void NullifySlowStatusEffect(this Player player, string hazardId) =>
        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                (int)status.EffectType, -1, -1, false, status.HazardId, false));
    }

    // SLOW EFFECT
    public static void ApplySlowEffect(this Player player)
    {
        player.TempData.IsSlowed = true;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
        (int)ItemEffectType.SlowStatusEffect, -1, -1, true, string.Empty, false));
    }

    public static void NullifySlowStatusEffect(this Player player)
    {
        if (player == null) return;

        player.TempData.IsSlowed = false;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                (int)ItemEffectType.SlowStatusEffect, -1, -1, false, string.Empty, false));

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
            (int)ItemEffectType.NullifySlowStatusEffect, 1, 1, true, string.Empty, false));
    }

    public static void RunUnderwaterTick(this Player player, ServerRConfig config)
    {
        if (player.Character.StatusEffects.GetEffect(ItemEffectType.WaterBreathing) > 0)
        {
            player.ResetUnderwaterTime();
            return;
        }

        var currentTime = player.Room.Time;

        if (player.TempData.UnderwaterTime == 0)
        {
            player.TempData.UnderwaterTime = currentTime;
            return;
        }

        if (currentTime - player.TempData.UnderwaterTime < config.BaseUnderwaterTime)
            return;

        player.ApplyDamageByPercent(0.1, ItemEffectType.WaterDamage, "0", 1);
        player.TempData.UnderwaterTime += 2.5f;
    }

    public static void ResetUnderwaterTime(this Player player) => player.TempData.UnderwaterTime = 0;

    public static bool HasNullifyEffect(this Player player, ItemCatalog itemCatalog) =>
     player.Character.Equipment.EquippedItems
         .Select(x => itemCatalog.GetItemFromId(x.Value))
         .Any(item => item != null && item.ItemEffects.Any(effect => effect.Type == ItemEffectType.NullifySlowStatusEffect));

    public static void StartPoisonDamage(this Player player, string hazardId, int damage, int hurtLength, ServerRConfig serverRConfig, TimerThread timerThread)
    {
        if (player == null || player.TempData.Invincible)
            return;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
        (int)ItemEffectType.PoisonDamage, damage, hurtLength, true, hazardId, false));

        player.ApplyCharacterDamage(damage, hazardId, hurtLength, serverRConfig, timerThread);
    }

    public static void TemporaryInvincibility(this Player player, double durationInSeconds)
    {
        if (durationInSeconds <= 0) return;

        player.Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, player.Room.Time,
                 (int)ItemEffectType.Invincibility, 0, (int)durationInSeconds, true, player.CharacterName, true));
    }

    public static StatusEffectData GetPoisonEffectData(this Player player, int poisonDamage, string hazardId,
        HazardRConfig hazardRConfig, TimerThread timerThread, int initDelay, int delay)
    {
        var damageCount = (int)Math.Ceiling((double)player.Character.CurrentLife / poisonDamage);
        var invincibilityDuration = -1;

        var hazardIsEnemy = player.Room.ContainsEnemy(hazardId);

        if (hazardIsEnemy)
        {
            poisonDamage /= hazardRConfig.PoisonDamageOverTimeDeduction;
            damageCount = hazardRConfig.PoisonDamageCountFromEnemy;
            initDelay = 0;
            delay = hazardRConfig.PoisonEffectInterval;
            invincibilityDuration = 1;
        }

        return new StatusEffectData()
        {
            Player = player,
            HazardDamage = poisonDamage,
            EffectType = ItemEffectType.PoisonDamage,
            HazardId = hazardId,
            InitialDamageDelay = initDelay,
            DamageDelay = delay,
            DamageCount = damageCount,
            InvincibilityDuration = invincibilityDuration,
            TimerThread = timerThread
        };
    }
}
