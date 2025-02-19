﻿using Server.Reawakened.Entities.Enemies.Behaviors;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.Abstractions;
using Server.Reawakened.XMLs.Data.Enemy.Enums;
using Server.Reawakened.XMLs.Data.Enemy.Models;

namespace Server.Reawakened.XMLs.Data.Enemy.States;
public class LookAroundState(LookAroundProperties properties, List<EnemyResourceModel> resources, StateType stateType) : BaseState(resources)
{
    public override AIBaseBehavior GetBaseBehaviour(BehaviorEnemy enemy) =>
        new AIBehaviorLookAround(properties, enemy, stateType);
}
