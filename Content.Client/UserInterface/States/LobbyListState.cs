using Content.Client.UserInterface.Hud;
using Content.Shared;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Timing;

namespace Content.Client.UserInterface.States;

public sealed class LobbyListState : State, IBeachBallState
{
    [Dependency] private readonly IUserInterfaceManager _userInterface = default!;

    private LobbyListHud? _lobbyListHud;

    public override void Startup()
    {
        _lobbyListHud = new LobbyListHud();
        LayoutContainer.SetAnchorAndMarginPreset(_lobbyListHud, LayoutContainer.LayoutPreset.Wide);
        _userInterface.StateRoot.AddChild(_lobbyListHud);
    }

    public override void Shutdown()
    {
        _lobbyListHud?.Dispose();
    }

    public void SubscribeToEvents(BeachballSystem system)
    {
        _lobbyListHud.SubscribeToEvents(system);
    }

    public void UnsubscribeFromEvents(BeachballSystem system)
    {
        _lobbyListHud.UnsubscribeFromEvents();
    }

    public void UpdateData(BeachballSystem system)
    {
        _lobbyListHud.UpdateLobbyList(system);
    }
}