using System;
using System.Collections.Generic;
using Content.Client.UserInterface.Hud;
using Content.Shared;
using Content.Shared.Paddle;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Client.UserInterface.States;

public sealed class GameState : State, IBeachBallState
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IUserInterfaceManager _userInterface = default!;
        
    private GameHud? _gameHud;
        
    public override void Startup()
    {
        _gameHud = new GameHud();
            
        LayoutContainer.SetAnchorAndMarginPreset(_gameHud, LayoutContainer.LayoutPreset.Wide);
        
        _userInterface.StateRoot.AddChild(_gameHud);
    }

    public override void Shutdown()
    {
        _gameHud?.Dispose();
    }

    public void SubscribeToEvents(BeachballSystem system)
    {
    }

    public void UnsubscribeFromEvents(BeachballSystem system)
    {
    }

    public void UpdateData(BeachballSystem system)
    {
        _gameHud.SetNames(system.GameState.Players);
        _gameHud.SetScore(system.GameState.Score);
    }
}