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
        args.State = new BeacherComponentState(component.Score, component.Player, component.First, component.Pressed, component.LastPress, component.DoubleBoostRemaining, component.PlayerBounds);
    }

    private void HandlePaddleState(EntityUid uid, BeacherComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not BeacherComponentState state)
            return;

        component.Score = state.Score;
        component.Player = state.Player;
        component.First = state.First;
        component.DoubleBoostRemaining = state.DoubleBoostRemaining;
        component.LastPress = state.LastPress;
        component.Pressed = state.Pressed;
        component.PlayerBounds = state.PlayerBounds;
    }
    
    private void SetMovementInput(ICommonSession? session, Button button, bool state)
    {
        if (session?.AttachedEntity == null 
            || Deleted(session.AttachedEntity) 
            || !TryComp<BeacherComponent>(session.AttachedEntity, out var paddle))
            return;

        if (state)
        {
            paddle.Pressed |= button;

            if (paddle.LastPress.Button == button &&
                _gameTiming.CurTick.Value - paddle.LastPress.Tick.Value <= SharedBeachballSystem.DoubleTickDelay)
            {
                paddle.DoubleBoostRemaining = SharedBeachballSystem.DoubleTickDuration;
            }
            paddle.LastPress = (button, _gameTiming.CurTick);
        }
        else
        {
            paddle.Pressed &= ~button;
        }

        paddle.Dirty();
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
    public Button Pressed { get; set; }
    public float DoubleBoostRemaining;
    public (Button Button, GameTick Tick) LastPress;
    public int Score { get; set; }
    public string Player { get; set; } = string.Empty;
    public bool First { get; set; }
    public (int Left, int Right) PlayerBounds;
}

[Serializable, NetSerializable]
public sealed class BeacherComponentState : ComponentState
{
    public int Score { get; }
    public string Player { get; }
    public bool First { get; }
    public Button Pressed { get; }
    
    public (Button Button, GameTick Tick) LastPress { get; }
    public float DoubleBoostRemaining { get; }
    public (int Left, int Right) PlayerBounds;

        
    public BeacherComponentState(int score, string player, bool first, Button pressed, (Button Button, GameTick Tick) lastPress, float doubleBoostRemaining, (int Left, int Right) playerBounds)
    {
        Score = score;
        Player = player;
        First = first;
        Pressed = pressed;
        LastPress = lastPress;
        DoubleBoostRemaining = doubleBoostRemaining;
        PlayerBounds = playerBounds;
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