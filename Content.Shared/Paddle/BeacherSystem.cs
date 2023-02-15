using System;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.IoC;
using Robust.Shared.Players;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared.Paddle;

/// <summary>
///     Controls player movement.
///     Since this is in shared, it will be predicted on the client and reconciled if needed.
/// </summary>
[UsedImplicitly]
public sealed class BeacherSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    
    public override void Initialize()
    {
        base.Initialize();
            
        SubscribeLocalEvent<BeacherComponent, ComponentGetState>(GetPaddleState);
        SubscribeLocalEvent<BeacherComponent, ComponentHandleState>(HandlePaddleState);
        
        CommandBinds.Builder
            .Bind(EngineKeyFunctions.MoveUp, new ButtonInputCmdHandler(Button.Up, SetMovementInput))
            .Bind(EngineKeyFunctions.MoveLeft, new ButtonInputCmdHandler(Button.Left, SetMovementInput))
            .Bind(EngineKeyFunctions.MoveRight, new ButtonInputCmdHandler(Button.Right, SetMovementInput))
            .Bind(EngineKeyFunctions.MoveDown, new ButtonInputCmdHandler(Button.Down, SetMovementInput))
            .Register<BeacherSystem>();
    }
    
    public override void Shutdown()
    {
        base.Shutdown();
        CommandBinds.Unregister<BeacherSystem>();
    }

    private void GetPaddleState(EntityUid uid, BeacherComponent component, ref ComponentGetState args)
    {
        args.State = new BeacherComponentState(component.Pressed, component.Player, component.TouchingFloor, component.LastPress, component.CanBoost, component.BoostCooldown);
    }

    private void HandlePaddleState(EntityUid uid, BeacherComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not BeacherComponentState state)
            return;

        component.Player = state.Player;
        component.Pressed = state.Pressed;
        component.TouchingFloor = state.TouchingFloor;
        component.CanBoost = state.CanBoost;
        component.BoostCooldown = state.BoostCooldown;
        component.LastPress = state.LastPress;
    }
    
    private void SetMovementInput(ICommonSession? session, Button button, bool state)
    {
        if (session?.AttachedEntity == null 
            || Deleted(session.AttachedEntity) 
            || !TryComp<BeacherComponent>(session.AttachedEntity, out var beacher))
            return;

        if (state)
        {
            beacher.Pressed |= button;
            if (beacher.LastPress.Button == button && beacher.LastPress.timeLeftForBoost > 0 && beacher.BoostCooldown == 0f)
            {
                Comp<PhysicsComponent>(session.AttachedEntity.Value).LinearVelocity *= 2;
                beacher.CanBoost = beacher.TouchingFloor;
                beacher.BoostCooldown = SharedBeachballSystem.BoostCoolDown;
            }
        }
        else
        {
            beacher.Pressed &= ~button;
            beacher.LastPress = (button, SharedBeachballSystem.MaxTimeBetweenBoostPress);
        }

        beacher.Dirty();
    }

    private sealed class ButtonInputCmdHandler : InputCmdHandler
    {
        public delegate void MoveDirectionHandler(ICommonSession? session, Button button, bool state);
            
        private readonly Button _button;
        private readonly MoveDirectionHandler _handler;
            
        public ButtonInputCmdHandler(Button button, MoveDirectionHandler handler)
        {
            _button = button;
            _handler = handler;
        }
            
        public override bool HandleCmdMessage(ICommonSession? session, InputCmdMessage message)
        {
            if (message is not FullInputCmdMessage full)
                return false;
                
            _handler.Invoke(session, _button, full.State == BoundKeyState.Down);
            return false;
        }
    }
}

[RegisterComponent, NetworkedComponent]
public sealed class BeacherComponent : Component
{
    public Button Pressed;
    public string Player = string.Empty;
    public bool TouchingFloor;
    public (Button Button, float timeLeftForBoost) LastPress;
    public bool CanBoost;
    public float BoostCooldown;
}

[Serializable, NetSerializable]
public sealed class BeacherComponentState : ComponentState
{
    public Button Pressed;
    public string Player;
    public bool TouchingFloor;
    public (Button Button, float timeLeftForBoost) LastPress;
    public bool CanBoost;
    public float BoostCooldown;

    public BeacherComponentState(Button pressed, string player, bool touchingFloor, (Button Button, float timeElapsed) lastPress, bool canBoost, float boostCooldown)
    {
        Pressed = pressed;
        Player = player;
        TouchingFloor = touchingFloor;
        LastPress = lastPress;
        CanBoost = canBoost;
        BoostCooldown = boostCooldown;
    }
}

[Flags, Serializable, NetSerializable]
public enum Button
{
    None = 0,
    Up = 1,
    Left = 2,
    Right = 4,
    Down = 8,
}