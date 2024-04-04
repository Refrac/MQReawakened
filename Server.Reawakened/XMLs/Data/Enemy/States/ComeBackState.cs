﻿using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.BehaviourTypes;
using Server.Reawakened.XMLs.Models.Enemy.Abstractions;
using Server.Reawakened.XMLs.Models.Enemy.Models;

namespace Server.Reawakened.XMLs.Models.Enemy.States;
public class ComeBackState(float comeBackSpeed, List<EnemyResourceModel> resources) : BaseState(resources)
{
    public float ComeBackSpeed => comeBackSpeed;

    protected override AIBaseBehavior GetBaseBehaviour(AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) => new AIBehaviorComeBack(this, globalComp);

    public override string[] GetStartArgs(BehaviorEnemy behaviorEnemy) =>
        [
            behaviorEnemy.Position.x.ToString(),
            behaviorEnemy.AiData.Intern_SpawnPosY.ToString()
        ];
}