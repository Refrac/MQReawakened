﻿using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;
public class LookAroundState(float lookTime, float startDirection, float forceDirection, float initialProgressRatio, bool snapOnGround, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float LookTime { get; } = lookTime;
    public float StartDirection { get; } = startDirection;
    public float ForceDirection { get; } = forceDirection;
    public float InitialProgressRatio { get; } = initialProgressRatio;
    public bool SnapOnGround { get; } = snapOnGround;

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorLookAround(this, globalComp);
}