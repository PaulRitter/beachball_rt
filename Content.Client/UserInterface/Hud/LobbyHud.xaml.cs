using Content.Shared;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.UserInterface.Hud;

[GenerateTypedNameReferences]
public sealed partial class LobbyHud : Control
{
    private BeachballSystem _system;
    void OnStartGamePressed(BaseButton.ButtonEventArgs x) => _system.StartLobby();

    public LobbyHud()
    {
        RobustXamlLoader.Load(this);

#if DEBUG
        StartGameButton.Disabled = false;
#endif
    }

    public void SubscribeToEvents(BeachballSystem system)
    {
        _system = system;

        StartGameButton.OnPressed += OnStartGamePressed;
    }
    
    public void UnsubscribeFromEvents()
    {
        _system = null;

        StartGameButton.OnPressed -= OnStartGamePressed;
    }
    
    public void SetLobbyData(NetworkedLobby lobby)
    {
        PlayerList.Clear();
        foreach (var lobbyPlayer in lobby.Players)
        {
            PlayerList.AddItem($"{lobbyPlayer.Key}{(lobbyPlayer.Value ? "- Admin" : "")}");
        }
    }

    public void SetStartGameEnabled(bool enabled)
    {
        StartGameButton.Disabled = !enabled;
    }
}