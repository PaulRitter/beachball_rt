using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Timing;

namespace Content.Shared.Paddle;

[UsedImplicitly]
public sealed class BeacherController : VirtualController
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    
    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<BeacherComponent, EndCollideEvent>(OnEndCollide);
        SubscribeLocalEvent<BeacherComponent, StartCollideEvent>(OnStartCollide);
    }
    
    private void OnStartCollide(EntityUid uid, BeacherComponent component, StartCollideEvent args)
    {
        if(args.OtherFixture.ID != SharedBeachballSystem.FloorFixtureId) return;

        component.TouchingFloor = true;
        component.CanBoost = true;
    }

    private void OnEndCollide(EntityUid uid, BeacherComponent component, EndCollideEvent args)
    {
        if(args.OtherFixture.ID != SharedBeachballSystem.FloorFixtureId) return;
        
        component.TouchingFloor = false;
    }

    public override void UpdateBeforeSolve(bool prediction, float frameTime)
    {
        base.UpdateBeforeSolve(prediction, frameTime);

        foreach (var (beacher, physics) in EntityManager.EntityQuery<BeacherComponent, PhysicsComponent>())
        {
            if (beacher.LastPress.timeLeftForBoost > 0f)
            {
                beacher.LastPress.timeLeftForBoost = MathF.Max(0f, beacher.LastPress.timeLeftForBoost -= frameTime);
            }

            if (beacher.BoostCooldown > 0f)
            {
                beacher.BoostCooldown = MathF.Max(0f, beacher.BoostCooldown -= frameTime);
            }
            
            var force = Vector2.Zero;
            
            if(beacher.TouchingFloor && (beacher.Pressed & Button.Up) != 0)
                force += Vector2.UnitY * SharedBeachballSystem.JumpSpeed;
            
            if((beacher.Pressed & Button.Left) != 0)
                force -= Vector2.UnitX * SharedBeachballSystem.PlayerSpeed;
            
            if((beacher.Pressed & Button.Right) != 0)
                force += Vector2.UnitX * SharedBeachballSystem.PlayerSpeed;
            
            if((beacher.Pressed & Button.Down) != 0)
                force -= Vector2.UnitY * SharedBeachballSystem.PlayerSpeed;

            physics.ApplyForce(force);

            
            physics.LinearVelocity =
                new Vector2(
                    Math.Clamp(physics.LinearVelocity.X, -SharedBeachballSystem.MaxHorizontalVelocity,
                        SharedBeachballSystem.MaxHorizontalVelocity), physics.LinearVelocity.Y);
                
            continue;
            
            if (beacher.LastPress.timeLeftForBoost > 0f && beacher.BoostCooldown == 0f)
            {
                beacher.BoostCooldown = SharedBeachballSystem.BoostCoolDown;
                //velocity *= 2;
                beacher.CanBoost = false;
            }

        }
    }
}