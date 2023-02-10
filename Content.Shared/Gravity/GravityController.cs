using Content.Shared.Ball;
using Content.Shared.Paddle;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Physics.Dynamics;

namespace Content.Shared.Gravity;

public sealed class GravityController : VirtualController
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;

    private float _ballMult;
    private float _playerMult;
    
    public override void Initialize()
    {
        base.Initialize();
        
        _configuration.OnValueChanged(ContentCVars.BallGravityMultiplier, val => _ballMult = val, true);
        _configuration.OnValueChanged(ContentCVars.PlayerGravityMultiplier, val => _playerMult = val, true);
    }
    
    public override void UpdateAfterSolve(bool prediction, float frameTime)
    {
        base.UpdateAfterSolve(prediction, frameTime);

        void ApplyGravity(TransformComponent transform, PhysicsComponent physics, float offset = 1f)
        {
            physics.ApplyForce(-Vector2.UnitY * 9.81f * 5 * offset);
        }
        
        foreach (var (_, transform, physics) in EntityManager
                     .EntityQuery<BallComponent, TransformComponent, PhysicsComponent>())
        {
            ApplyGravity(transform, physics, _ballMult); //tweak param
        }
        
        foreach (var (_, transform, physics) in EntityManager
                     .EntityQuery<BeacherComponent, TransformComponent, PhysicsComponent>())
        {
            ApplyGravity(transform, physics, _playerMult);
        }
    }
}