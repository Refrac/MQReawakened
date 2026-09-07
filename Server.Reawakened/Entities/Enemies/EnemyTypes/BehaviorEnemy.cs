using A2m.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Base.Timers.Services;
using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.Behaviors;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes.Abstractions;
using Server.Reawakened.Entities.Enemies.Extensions;
using Server.Reawakened.Entities.Enemies.Models;
using Server.Reawakened.Entities.Enemies.Services;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.Models;
using Server.Reawakened.XMLs.Data.Enemy.States;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Server.Reawakened.Entities.Enemies.EnemyTypes;

public class BehaviorEnemy : BaseEnemy
{
    [ThreadStatic]
    private static List<Player> s_playerBuffer;

    public AIStatsGlobalComp Global;
    public AIStatsGenericComp Generic;
    public AIProcessData AiData;

    public Dictionary<StateType, AIBaseBehavior> Behaviors;

    public StateType CurrentState;
    public AIBaseBehavior CurrentBehavior;

    private float _lastUpdate;
	
    private StateType _attackBehavior;

    public BehaviorEnemy(EnemyData data) : base(data)
    {
    }

    public override void Initialize()
    {
        Global = Room.GetEntityFromId<AIStatsGlobalComp>(Id);
        Generic = Room.GetEntityFromId<AIStatsGenericComp>(Id);

        if (!IsFromSpawner && Generic is not null)
            Generic.SetDefaultPatrolRange();

        EnemyModel.GlobalProperties?.ApplyGlobalPropertiesFromModel(Global);
        EnemyModel.GenericScript?.ApplyGenericPropertiesFromModel(Global);

        AiData = new AIProcessData
        {
            Intern_SpawnPosX = Position.X,
            Intern_SpawnPosY = Position.Y,
            Intern_SpawnPosZ = Position.Z,
            Sync_PosX = Position.X,
            Sync_PosY = Position.Y,
            Sync_PosZ = Position.Z,
            SyncInit_Dir = 0,
            SyncInit_ProgressRatio = Generic.Patrol_InitialProgressRatio
        };

        AiData.SetStats(Global.GetGlobalProperties());

        AiData.services = new AIServices
        {
            _shoot = new Shooter(this),
            _bomber = new Bomber(this),
            _scan = new Scanner(this),
            _collision = new Collisions(this),
            _suicide = new Runnable(this)
        };

        _attackBehavior = Global.AttackBehavior;

        // Optimized dictionary allocation replacing LINQ ToDictionary
        var behaviorData = EnemyModel.BehaviorData;
        Behaviors = new Dictionary<StateType, AIBaseBehavior>(behaviorData.Count);
        foreach (var kvp in behaviorData)
        {
            Behaviors.Add(kvp.Key, kvp.Value.GetBaseBehaviour(this));
        }

        base.Initialize();

        Room.SendSyncEvent(
            AISyncEventHelper.AIInit(
                Position.X, Position.Y, Position.Z,
                Position.X, Position.Y,
                Generic?.Patrol_InitialProgressRatio ?? 0f, this
            )
        );

        ChangeBehavior(StateType.Patrol, Position.X, Position.Y, AiData.Intern_Dir);
    }

    public override void InternalUpdate()
    {
        Position.SetPosition(
            AiData.Sync_PosX,
            AiData.Sync_PosY,
            AiData.Sync_PosZ
        );

        var hasDetected = false;

        if (CurrentBehavior.ShouldDetectPlayers)
            hasDetected = HasDetectedPlayers();

        if (!hasDetected)
        {
            if (CurrentBehavior.TryUpdate())
            {
                if (AiData.Intern_FireProjectile)
                    FireProjectile(false);
            }

            if (Global != null && CurrentState is var state &&
                (state == Global.AwareBehavior || state is StateType.LookAround or StateType.Acting))
            {
                if (Room.Time >= _lastUpdate + CurrentBehavior.GetBehaviorTime())
                    CurrentBehavior.NextState();
            }
        }
    }

    public bool HasDetectedPlayers()
    {
        if (!this.TryGetDetectionCollider(out var enemyCollider))
            return false;

        var myPlane = ParentPlane;
        var minX = AiData.Intern_MinPointX;
        var maxX = AiData.Intern_MaxPointX;
        var limitByPatrol = Global.Global_DetectionLimitedByPatrolLine;

        foreach (var player in Room.GetPlayers())
        {
            if (player?.Character == null)
                continue;

            var character = player.Character;
            if (character.CurrentLife <= 0)
                continue;

            if (character.StatusEffects.HasEffect(ItemEffectType.Invisibility))
                continue;

            if (myPlane != player.GetPlayersPlaneString())
                continue;

            var temp = player.TempData;
            if (temp == null)
                continue;

            if (limitByPatrol && (temp.Position.X <= minX || temp.Position.X >= maxX))
                continue;

            var playerCollider = temp.PlayerCollider;
            if (playerCollider != null && enemyCollider.CheckCollision(playerCollider))
            {
                EnemyAggroPlayer(player);
                return true;
            }
        }

        return false;
    }

    public void FireProjectile(bool isGrenade)
    {
        var position = new Vector3Model(
            Position.X + AiData.Intern_Dir * Global.Global_ShootOffsetX,
            Position.Y + Global.Global_ShootOffsetY,
            Position.Z
        );

        var speed = new Vector2(
            (float)Math.Cos(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed,
            (float)Math.Sin(AiData.Intern_FireAngle) * AiData.Intern_FireSpeed
        );

        FireProjectile(position, speed, isGrenade);

        AiData.Intern_FireProjectile = false;
    }

    public void ChangeBehavior(StateType behaviourType, float targetX, float targetY, int direction)
    {
        if (AiData.Intern_PendingSpeedFactor >= 0f)
        {
            AiData.Sync_SpeedFactor = AiData.Intern_PendingSpeedFactor;
            AiData.Intern_AnimSpeed = AiData.Intern_PendingSpeedFactor;
            AiData.Intern_PendingSpeedFactor = -1f;
        }

        CurrentBehavior?.Stop();

        CurrentBehavior = Behaviors[behaviourType];
        CurrentState = behaviourType;

        Behaviors[behaviourType].Start();

        _lastUpdate = Room.Time;

        Room.SendSyncEvent(
            AISyncEventHelper.AIDo(
                Position.X, Position.Y, 1.0f,
                targetX, targetY, direction, Global != null && CurrentState == Global.AwareBehavior,
                this
            )
        );
    }

    public override void Damage(Player player, int damage)
    {
        base.Damage(player, damage);
        if (CurrentBehavior.ShouldAggroOnHit)
            EnemyAggroPlayer(player);
    }

    public override void PetDamage(Player player)
    {
        base.PetDamage(player);
        if (CurrentBehavior.ShouldAggroOnHit)
            EnemyAggroPlayer(player);
    }

    public override void SendAiData(Player player, bool sendAIDo)
    {
        var ratio = 0.0f;

        if (AiData is null)
        {
            Logger.LogError("AiData for enemy {Id} was null! Skipping this enemy...", Id);
            return;
        }

        if (CurrentBehavior is not null)
            ratio = CurrentBehavior.GetBehaviorRatio(Room.Time);

        player.SendSyncEventToPlayer(
            AISyncEventHelper.AIInit(
                AiData.Sync_PosX, AiData.Sync_PosY, AiData.Sync_PosZ,
                AiData.Intern_SpawnPosX, AiData.Intern_SpawnPosY,
                ratio, this
            )
        );

        if (sendAIDo)
            player.SendSyncEventToPlayer(
                AISyncEventHelper.AIDo(
                    AiData.Sync_PosX, AiData.Sync_PosY, 1.0f,
                    AiData.Sync_TargetPosX, AiData.Sync_TargetPosY, AiData.Intern_Dir, Global != null && CurrentState == Global.AwareBehavior,
                    this
                )
            );
        }

    public void EnemyAggroPlayer(Player player)
    {
        if (player == null)
        {
            Logger.LogError("Could not find player that damaged {PrefabName}! Returning...", PrefabName);
            return;
        }

        Logger.LogTrace("Enemy {PrefabName} aggroed on player {PlayerName}", PrefabName, player.CharacterName);

        AiData.Sync_TargetPosX = player.TempData.Position.X;
        AiData.Sync_TargetPosY = player.TempData.Position.Y;

        ChangeBehavior(
            Global.AttackBehavior,
            player.TempData.Position.X, player.TempData.Position.Y,
            Generic.Patrol_ForceDirectionX
        );
    }
}
