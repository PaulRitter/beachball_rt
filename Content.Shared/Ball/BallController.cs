using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Player;

namespace Content.Shared.Ball;

[UsedImplicitly]
public sealed class BallController : VirtualController
{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    
    public override void Initialize()
    {
        base.Initialize();
        
        //UpdatesBefore.Add(typeof(ArenaController));
        SubscribeLocalEvent<BallComponent, StartCollideEvent>(OnCollideStartEvent);
    }

    private void OnCollideStartEvent(EntityUid uid, BallComponent component, StartCollideEvent args)
    {
        if(args.OtherFixture.ID != "Player") return;
        
        Comp<BallComponent>(uid).Frozen = false;
        _audioSystem.PlayGlobal("/Audio/bloop.wav", Filter.Broadcast(), AudioParams.Default.WithVolume(-5f));

    }

    public override void UpdateAfterSolve(bool prediction, float frameTime)
    {
        base.UpdateAfterSolve(prediction, frameTime);
        foreach (var (ball, physics) in EntityManager.EntityQuery<BallComponent, PhysicsComponent>())
        {
            if (ball.Frozen)
            {
                physics.ResetDynamics();
            }
        }
    }
}

[RegisterComponent]
public sealed class BallComponent : Component
{
    public bool Frozen;
}