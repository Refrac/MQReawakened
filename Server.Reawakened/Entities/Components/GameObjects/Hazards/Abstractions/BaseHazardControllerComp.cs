using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Core.Abstractions;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Entities.Colliders;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.Components.GameObjects.Breakables;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.Rooms.Models.Timers;
using Server.Reawakened.XMLs.Bundles.Base;
using static Server.Reawakened.Players.Extensions.PlayerStatusEffectExtensions;

namespace Server.Reawakened.Entities.Components.GameObjects.Hazards.Abstractions;

public abstract class BaseHazardControllerComp<T> : Component<T> where T : HazardController
{
    public string HurtEffect => ComponentData.HurtEffect;
    public float HurtLength => ComponentData.HurtLenght;
    public float InitialDamageDelay => ComponentData.InitialDamageDelay;
    public float DamageDelay => ComponentData.DamageDelay;
    public bool DeathPlane => ComponentData.DeathPlane;
    public string NullifyingEffect => ComponentData.NullifyingEffect;
    public bool HitOnlyVisible => ComponentData.HitOnlyVisible;
    public float InitialProgressRatio => ComponentData.InitialProgressRatio;
    public float ActiveDuration => ComponentData.ActiveDuration;
    public float DeactivationDuration => ComponentData.DeactivationDuration;
    public float HealthRatioDamage => ComponentData.HealthRatioDamage;
    public int HurtSelfOnDamage => ComponentData.HurtSelfOnDamage;

    public ItemEffectType EffectType = ItemEffectType.Unknown;

    public bool IsActive = true;
    public bool TimedHazard = false;

    public int Damage;

    private IEnemyController _enemyController;
    private float _activationStartTime = 0;
    private HazardEffectCollider _collider;

    public TimerThread TimerThread { get; set; }
    public HazardRConfig HazardRConfig { get; set; }
    public ServerRConfig ServerRConfig { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ILogger<BaseHazardControllerComp<HazardController>> Logger { get; set; }

    public override object[] GetInitData(Player player) => [0];

    public override void InitializeComponent()
    {
        _enemyController = Room.GetEnemyFromId(Id);

        //Activate timed hazards.
        if (ActiveDuration > 0 && DeactivationDuration > 0)
            TimedHazard = true;

        if (!Enum.TryParse(HurtEffect, true, out EffectType))
        {
            if (PrefabName.Contains("ToxicCloud"))
            {
                EffectType = ItemEffectType.PoisonDamage;
            }
            else if (PrefabName.Contains("Deathplane"))
            {
                EffectType = ItemEffectType.BluntDamage;
            }
            else
            {
                switch (HurtEffect)
                {
                    case "NoEffect":
                        EffectType = ItemEffectType.Unknown;
                        break;
                    case "StandardDamage":
                        EffectType = ItemEffectType.BluntDamage;
                        break;
                    default:
                        Logger.LogError("Could not find effect type of: '{effect}' for '{prefabName}' ({id})!", HurtEffect, PrefabName, Id);
                        break;
                }
            }
        }

        //Prevents enemies and hazards sharing same collider Ids.
        if (_enemyController == null)
        {
            //Prevents spider webs from inaccurately adjusting collider positioning.
            if (EffectType == ItemEffectType.SlowStatusEffect)
                Rectangle.X = 0;

            _collider = new HazardEffectCollider(this, Logger);
            
            Logger.LogInformation("Created Hazard Collider with ID: {Id} for Prefab: {PrefabName}", Id, PrefabName);
        }
    }

    public override void Update()
    {
        if (Room == null)
            return;

        base.Update();

        if (TimedHazard)
        {
            var time = Room.Time;
            if (IsActive && time - _activationStartTime > ActiveDuration)
            {
                _activationStartTime += ActiveDuration;
                IsActive = false;
            }
            else if (!IsActive && time - _activationStartTime > DeactivationDuration)
            {
                _activationStartTime += DeactivationDuration;
                IsActive = true;
            }
        }

        _collider?.RunCollisionDetection();
    }
    
    public override void NotifyCollision(NotifyCollision_SyncEvent notifyCollisionEvent, Player player)
    {
    }

    public void ApplyHazardEffect(Player player)
    {
        if (player == null)
            return;

        if (player.TempData.Invincible || player.TempData.IsKnockedOut)
            return;

        if ((TimedHazard || EffectType == ItemEffectType.WaterBreathing) && !IsActive)
            return;

        if (HitOnlyVisible && player.Character.StatusEffects.HasEffect(ItemEffectType.Invisibility))
            return;

        if (DeathPlane && Room.Time < 20)
            return;

        // Reduces slow status effect spam.
        if ((EffectType == ItemEffectType.SlowStatusEffect || !player.TempData.OnGround) &&
            player.TempData.IsSlowed)
            return;
        
        if (EffectType == ItemEffectType.SlowStatusEffect && player.HasNullifyEffect(ItemCatalog))
            return;

        Damage = (int)Math.Ceiling(player.Character.MaxLife * HealthRatioDamage);

        var enemy = Room.GetEnemy(Id);

        if (enemy != null)
        {
            EffectType = enemy.EnemyController.EnemyEffectType;

            Damage = EnemyDamagePlayer(player, enemy);
        }

        Logger.LogTrace("Applying {statusEffect} to {characterName} from {prefabName}", EffectType, player.CharacterName, PrefabName);

        switch (EffectType)
        {
            // Some hazards actually use Unknown, so don't discern by this one
            //case ItemEffectType.Unknown:
            //    return;
            case ItemEffectType.SlowStatusEffect:
                ApplySlowEffect(player);
                break;
            case ItemEffectType.BluntDamage:
            case ItemEffectType.FireDamage:
            case ItemEffectType.AirDamage:
            case ItemEffectType.EarthDamage:
            case ItemEffectType.IceDamage:
            case ItemEffectType.LightningDamage:
                player.ApplyCharacterDamage(Damage, EffectType, Id, Convert.ToInt32(DamageDelay));
                break;
            case ItemEffectType.PoisonDamage:
                if (!player.TempData.IsPoisoned)
                {
                    ApplyPoisonDamage(player.GetPoisonEffectData(Damage, Id, HazardRConfig,
                        TimerThread, Convert.ToInt32(InitialDamageDelay), Convert.ToInt32(DamageDelay)));
                }
                break;
            case ItemEffectType.WaterBreathing:
                ApplyWaterBreathing(player);
                break;
            case ItemEffectType.SpiderWeb:
                Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time, (int)ItemEffectType.SpiderWeb, 1, 1, true, Id, false));
                
                var breakable = Room.GetEntityFromId<BreakableEventControllerComp>(Id);
                breakable?.Damage(breakable.Damageable.CurrentHealth, ItemEffectType.BluntDamage, player);
                break;
            default:
                Logger.LogInformation("Unknown status effect {statusEffect} from {prefabName}", HurtEffect, PrefabName);

                Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time, (int)ItemEffectType.BluntDamage, 1, 1, true, Id, false));
                player.ApplyCharacterDamage(Damage, EffectType, Id, Convert.ToInt32(DamageDelay));

                break;
        }
    }

    public int EnemyDamagePlayer(Player player, BaseEnemy enemy) => enemy.EnemyDamagePlayer(player);

    private void ActivateHazardDelay(ITimerData data)
    {
        if (data is not BaseHazardControllerComp<T> hazard)
            return;

        hazard.IsActive = true;
    }

    // WATER BREATHING
    public void ApplyWaterBreathing(object playerData)
    {
        if (playerData == null || playerData is not Player player)
            return;

        Room.SendSyncEvent(new StatusEffect_SyncEvent(player.GameObjectId, Room.Time,
                    (int)ItemEffectType.WaterBreathing, 1, 1, true, Id, false));

        IsActive = false;

        TimerThread.RunDelayed(ActivateHazardDelay, this, TimeSpan.FromSeconds(1));

        player.ResetUnderwaterTime();

        Logger.LogInformation("Reset underwater timer for {characterName}", player.CharacterName);
    }

    // SLOW EFFECT
    private void ApplySlowEffect(Player player)
    {
        player.ApplySlowEffect();

        player.TempData.IsSlowed = true;

        TimerThread.RunDelayed(DisableSlowEffect, new PlayerTimer() { Player = player }, TimeSpan.FromSeconds(HazardRConfig.SlowEffectInterval));
    }

    private void DisableSlowEffect(ITimerData data)
    {
        if (data is not PlayerTimer playerTimer)
            return;

        var player = playerTimer.Player;

        if (_collider.CheckCollision(player.GetCollider()))
            player.NullifySlowStatusEffect();
    }
}
