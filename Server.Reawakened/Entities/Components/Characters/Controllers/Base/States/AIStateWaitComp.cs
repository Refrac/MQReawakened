using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStateWaitComp : BaseAIState<AIStateWaitMQR, AI_State>
{
    public override string StateName => "AIStateWait";

    public float FxWaitDuration = 0;
    public float WaitDuration => ComponentData.WaitDuration + FxWaitDuration;

    public float WaitTime { get; private set; } = 0;

    public override void StartState(float time = -1) => base.StartState(WaitDuration);

    public override AI_State GetInitialAIState() => new([], loop: false);

    public override void OnAIStateIn() => WaitTime = Room.Time + WaitDuration;
}
