using System;
using Content.Client.UserInterface.Hud;
using Content.Shared;
using Content.Shared.Paddle;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Timing;

namespace Content.Client.UserInterface.States;

public sealed class GameState : State
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IUserInterfaceManager _userInterface = default!;
        
    private GameHud? _gameHud;
        
    public override void Startup()
    {
        _gameHud = new GameHud() { Visible = false };
            
        LayoutContainer.SetAnchorAndMarginPreset(_gameHud, LayoutContainer.LayoutPreset.Wide);
            
        _userInterface.StateRoot.AddChild(_gameHud);
    }

    public override void Shutdown()
    {
        _gameHud?.Dispose();
    }

    public override void FrameUpdate(FrameEventArgs e)
    {
        base.FrameUpdate(e);

        if (_gameHud == null)
            return;

        var state = EntitySystem.Get<BeachballSystem>().GameState;
        
        if(state == null) return;

        var gameEnded = false;

        if (!gameEnded)
            _gameHud.WinnerLabelText = string.Empty;

        var winningScore = 0;

        foreach (var paddle in _entityManager.EntityQuery<BeacherComponent>())
        {
            // There's only supposed to be two paddle entities.
            if (paddle.First)
            {
                _gameHud.PlayerOneName = paddle.Player;
                _gameHud.PlayerOneScore = paddle.Score;
            }
            else
            {
                _gameHud.PlayerTwoName = paddle.Player;
                _gameHud.PlayerTwoScore = paddle.Score;
            }
                
            if (!gameEnded)
                continue;

            if (paddle.Score == winningScore)
            {
                _gameHud.WinnerLabelText = "It's a draw!";
                continue;
            }

            if (paddle.Score >= winningScore)
            {
                winningScore = paddle.Score;
                _gameHud.WinnerLabelText = $"{paddle.Player} wins!";
            }
        }
    }
}