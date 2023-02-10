using Content.Shared.Ball;
using Content.Shared.Paddle;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Physics.Dynamics;

namespace Content.Shared.Gravity;

public sealed class GravityController : VirtualController
{
    public override void UpdateAfterSolve(bool prediction, float frameTime)
    {
        base.UpdateAfterSolve(prediction, frameTime);

        void ApplyGravity(TransformComponent transform, PhysicsComponent physics, float offset = 1f)
        {
            if (transform.WorldPosition.Y <= -15)
            {
                transform.WorldPosition = new Vector2(transform.WorldPosition.X, -15);
                return;
            }
            physics.ApplyForce(-Vector2.UnitY * 9.81f * 5 * offset);
        }
        
        foreach (var (_, transform, physics) in EntityManager
                     .EntityQuery<BallComponent, TransformComponent, PhysicsComponent>())
        {
            ApplyGravity(transform, physics, 0.7f);
        }
        
        foreach (var (_, transform, physics) in EntityManager
                     .EntityQuery<BeacherComponent, TransformComponent, PhysicsComponent>())
        {
            ApplyGravity(transform, physics);
        }
    }
}