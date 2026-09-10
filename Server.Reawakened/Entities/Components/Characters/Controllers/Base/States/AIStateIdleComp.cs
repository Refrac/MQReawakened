using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStateIdleComp : BaseAIState<AIStateIdleMQR, AI_State>
{
    public override string StateName => "AIStateIdle";

    public override AI_State GetInitialAIState() => new([], loop: true);

    public override void InitializeComponent()
    {
        base.InitializeComponent();
    }
}
