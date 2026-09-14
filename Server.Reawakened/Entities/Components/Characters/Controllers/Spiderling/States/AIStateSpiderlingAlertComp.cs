using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Spiderling.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Spiderling.States;
public class AIStateSpiderlingAlertComp : BaseAIState<AIStateSpiderlingAlertMQR, AI_State>
{
    public override string StateName => "AIStateSpiderlingAlert";
    
    public float AlertTime => ComponentData.AlertTime;

    public float WaitTime { get; private set; } = 0;

    public override void StartState(float time = -1) => base.StartState(AlertTime);

    public override AI_State GetInitialAIState() => new([], loop: false);

    public override void OnAIStateIn() => WaitTime = Room.Time + AlertTime;
}
