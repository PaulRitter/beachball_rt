using System.Linq;
using Content.Client.UserInterface.Hud;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Timing;

namespace Content.Client.UserInterface.States;

public sealed class LobbyState : State, IBeachBallState
{
    [Dependency] private readonly IUserInterfaceManager _userInterface = default!;

    private LobbyHud? _lobbyHud;

    public override void Startup()
    {
        _lobbyHud = new LobbyHud();
        LayoutContainer.SetAnchorAndMarginPreset(_lobbyHud, LayoutContainer.LayoutPreset.Wide);
        _userInterface.StateRoot.AddChild(_lobbyHud);
    }

    public override void Shutdown()
    {
        _lobbyHud?.Dispose();
    }

    public void SubscribeToEvents(BeachballSystem system)
    {
        _lobbyHud.SubscribeToEvents(system);
    }

    public void UnsubscribeFromEvents(BeachballSystem system)
    {
        _lobbyHud.UnsubscribeFromEvents();
    }

    public void UpdateData(BeachballSystem system)
    {
        _lobbyHud.SetLobbyData(system.Lobbies.First(x => x.Name == system.selectedLobby));
    }
}