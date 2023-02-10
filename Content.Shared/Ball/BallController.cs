using System;
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
    [Dependency] private readonly SharedBeachballSystem _beachballSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<BallComponent, StartCollideEvent>(OnStartCollide);
    }

    private void OnStartCollide(EntityUid uid, BallComponent component, StartCollideEvent args)
    {
        switch (args.OtherFixture.ID)
        {
            
        }

        switch (args.OtherFixture.ID)
        {
            case "Floor":
                var transform = Transform(uid);

                var scoredIndex = transform.WorldPosition.X < 0 ? 1 : 0;
                _beachballSystem.OnScored(transform.MapID, scoredIndex);
                break;
            case "Player":
                Comp<BallComponent>(uid).Frozen = false;
                _audioSystem.PlayGlobal("/Audio/bloop.wav", Filter.Broadcast(), AudioParams.Default.WithVolume(-5f));
                args.OurFixture.Body.ResetDynamics();
                var bounceVector = (Transform(uid).WorldPosition - Transform(args.OtherFixture.Body.Owner).WorldPosition)
                    .Normalized * args.OtherFixture.Body.LinearVelocity.Length;
                args.OurFixture.Body.LinearVelocity = (bounceVector + args.OurFixture.Body.LinearVelocity + args.OtherFixture.Body.LinearVelocity) / 3f * 2f; //tweak param
                break;
            case "LeftWall":
                if (args.OurFixture.Body.LinearVelocity.X < 0f) //its moving into the wall
                {
                    args.OurFixture.Body.LinearVelocity = new Vector2(args.OurFixture.Body.LinearVelocity.X * -1f,
                        args.OurFixture.Body.LinearVelocity.Y);
                }
                break;
            case "RightWall":
                if (args.OurFixture.Body.LinearVelocity.X > 0f) //its moving into the wall
                {
                    args.OurFixture.Body.LinearVelocity = new Vector2(args.OurFixture.Body.LinearVelocity.X * -1f,
                        args.OurFixture.Body.LinearVelocity.Y);
                }
                break;
        }
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