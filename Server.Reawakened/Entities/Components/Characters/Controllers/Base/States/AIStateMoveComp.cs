using A2m.Server;
using Server.Reawakened.Entities.Components.Characters.Controllers.Base.Abstractions;
using Server.Reawakened.Entities.DataComponentAccessors.Base.States;
using static A2m.Server.ExtLevelEditor;

namespace Server.Reawakened.Entities.Components.Characters.Controllers.Base.States;
public class AIStateMoveComp : BaseAIState<AIStateMoveMQR, AI_State_Move>
{
    public override string StateName => "AIStateMove";

    public float MovementSpeed => ComponentData.MovementSpeed;

    private readonly vector3 _dampingVelocity = new(0f, 0f, 0f);

    private float _spawnX;
    private float _spawnY;
    private float _spawnZ;
    
    public vector3 TargetPosition = new(0f, 0f, 0f);

    public float EndTime;

    public override void InitializeComponent()
    {
        base.InitializeComponent();

        _spawnX = Position.X;
        _spawnY = Position.Y;
        _spawnZ = Position.Z;
    }
    
    protected override ComponentSettings GetStartSettings() => ["ST", Room.Time.ToString()];
    
    public override AI_State_Move GetInitialAIState() => new (MovementSpeed);

    public override ComponentSettings GetSettings() => [
        _spawnX.ToString(), _spawnY.ToString(), _spawnZ.ToString(),
        TargetPosition.x.ToString(), TargetPosition.y.ToString(), TargetPosition.z.ToString()
    ];

    public override void OnAIStateIn()
    {
        EndTime = Room.Time + 0.5f;
        
        var spawnPosition = new vector3(_spawnX, _spawnY, _spawnZ);
        
        State.Init(spawnPosition, TargetPosition);
        StateMachine.SetForceDirectionX(Convert.ToInt32(TargetPosition.x - spawnPosition.x));
    }

    public override void Execute()
    {
        var position = Position.ToVector3();
        var currentPosition = State.CurrentPosition;
        var dampenedPosition = new vector3(0f, 0f, 0f);
        var springK = 200f;

        MathUtils.CriticallyDampedSpring1D(springK, position.x, currentPosition.x, ref _dampingVelocity.x, ref dampenedPosition.x, Room.DeltaTime);
        MathUtils.CriticallyDampedSpring1D(springK, position.y, currentPosition.y, ref _dampingVelocity.y, ref dampenedPosition.y, Room.DeltaTime);
        MathUtils.CriticallyDampedSpring1D(springK, position.z, currentPosition.z, ref _dampingVelocity.z, ref dampenedPosition.z, Room.DeltaTime);

        Position.SetPosition(dampenedPosition);
    }
}
