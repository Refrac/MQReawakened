﻿using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Base.States;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStateStunnedComp : BaseAIState<AIStateStunnedMQR>
{
    public override string StateName => "AIStateStunned";
}
