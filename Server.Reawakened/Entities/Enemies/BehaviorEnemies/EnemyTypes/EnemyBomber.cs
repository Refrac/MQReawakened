﻿using Server.Reawakened.Entities.Components;
using Server.Reawakened.Entities.Enemies.BehaviorEnemies.Abstractions;
using Server.Reawakened.Rooms;
using Server.Reawakened.XMLs.Models.Enemy.Enums;

namespace Server.Reawakened.Entities.Enemies.BehaviorEnemies.EnemyTypes;
public class EnemyBomber(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : BehaviorEnemy(room, entityId, prefabName, enemyController, services)
{
    public override void HandleAggro()
    {
        base.HandleAggro();

        if (!AiBehavior.Update(ref AiData, Room.Time))
            ChangeBehavior(StateTypes.Bomber, AiData.Sync_TargetPosX, AiData.Sync_TargetPosY, AiData.Intern_Dir);
    }

    public override void HandleBomber()
    {
        base.HandleBomber();

        if (Room.Time >= BehaviorEndTime)
            base.Damage(EnemyController.MaxHealth, null);
    }
}