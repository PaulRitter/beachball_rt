using JetBrains.Annotations;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Controllers;
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
    }

    public override void UpdateAfterSolve(bool prediction, float frameTime)
    {
        base.UpdateAfterSolve(prediction, frameTime);

        foreach (var (_, transform, physics) in EntityManager.EntityQuery<BallComponent, TransformComponent, PhysicsComponent>())
        {
            var bounced = false;

            if (transform.WorldPosition.Y <= 0)
            {
                physics.LinearVelocity *= new Vector2(1, -1);
                bounced = true;
            }
            
            var x = transform.WorldPosition.X;

            if ((x <= SharedBeachballSystem.FieldBounds.left && physics.LinearVelocity.X < 0) ||
                (x >= SharedBeachballSystem.FieldBounds.right && physics.LinearVelocity.X > 0))
            {
                physics.LinearVelocity *= new Vector2(-1, 1);
                bounced = true;
            }

            if (bounced)
            {
                _audioSystem.PlayGlobal("/Audio/bloop.wav", Filter.Broadcast(), AudioParams.Default.WithVolume(-5f));
            }
            
            //todo paul apply gravity
        }
    }
}

[RegisterComponent]
public sealed class BallComponent : Component {}