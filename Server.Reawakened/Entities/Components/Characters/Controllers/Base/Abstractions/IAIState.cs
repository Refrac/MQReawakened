using Server.Reawakened.Entities.Enemies.EnemyTypes;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;

public interface IAIState
{
    string StateName { get; }

    void StartState(float time = -1);
    void UpdateState();
    void StopState(float time = 0);

    ComponentSettings GetFullSettings();
    void SetStateMachine(IAIStateMachine machine);
    void SetEnemyController(AIStateEnemy enemyController);
}
