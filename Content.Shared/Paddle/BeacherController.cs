using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Timing;

namespace Content.Shared.Paddle;

[UsedImplicitly]
public sealed class BeacherController : VirtualController
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    
    public override void UpdateBeforeSolve(bool prediction, float frameTime)
    {
        base.UpdateBeforeSolve(prediction, frameTime);

        foreach (var (paddle, transformComponent, physics) in EntityManager.EntityQuery<BeacherComponent, TransformComponent, PhysicsComponent>())
        {
            var force = Vector2.Zero;
            
            if((paddle.Pressed & Button.Up) != 0)
                force += Vector2.UnitY;
            
            if((paddle.Pressed & Button.Left) != 0)
                force -= Vector2.UnitX;
            
            if((paddle.Pressed & Button.Right) != 0)
                force += Vector2.UnitX;
            
            if((paddle.Pressed & Button.Down) != 0)
                force -= Vector2.UnitY;

            if (paddle.DoubleBoostRemaining > 0)
            {
                paddle.DoubleBoostRemaining -= frameTime;
                force *= 2;
            }

            physics.ApplyForce(force * SharedBeachballSystem.PlayerSpeed);

            if (transformComponent.WorldPosition.X > SharedBeachballSystem.FieldBounds.right / 2f)
            {
                transformComponent.WorldPosition = new Vector2(SharedBeachballSystem.FieldBounds.right / 2f, transformComponent.WorldPosition.Y);
            }
        }
    }
}