using System.Collections.Generic;
using Content.Client.UserInterface.States;
using Content.Shared;
using Content.Shared.ClientMessages;
using Content.Shared.ServerMessages;
using Robust.Client.GameObjects;
using Robust.Client.State;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Network;

namespace Content.Client;

public sealed class BeachballSystem : SharedBeachballSystem
{
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly IClientNetManager _clientNetManager = default!;
    
    public List<NetworkedLobby> Lobbies = new();
    public string? selectedLobby;
    public NetworkedBeachballGame? GameState;
    public BeachballPlayerState PlayerState;
    private EntityUid? _cameraUid;
    
    public override void Initialize()
    {
        base.Initialize();
            
        SubscribeNetworkEvent<LobbyListMessage>(OnLobbyChanged);
        SubscribeNetworkEvent<GameCreatedMessage>(OnGameCreated);
        SubscribeNetworkEvent<ScoreUpdate>(OnScored);
        SubscribeNetworkEvent<LobbyJoinedMessage>(OnLobbyJoined);
        SubscribeNetworkEvent<LobbyLeftMessage>(OnLobbyLeft);
        
        SubscribeLocalEvent<PlayerAttachSysMessage>(OnPlayerAttached);
        
        _stateManager.OnStateChanged += StateManagerOnOnStateChanged;
        if (_stateManager.CurrentState is IBeachBallState beachBallState)
        {
            beachBallState.SubscribeToEvents(this);
            beachBallState.UpdateData(this);
        }
    }

    public override void Shutdown()
    {
        base.Shutdown();
        
        _stateManager.OnStateChanged -= StateManagerOnOnStateChanged;
    }

    private void OnLobbyJoined(LobbyJoinedMessage ev)
    {
        selectedLobby = ev.Name;
        _stateManager.RequestStateChange<LobbyState>();
    }

    private void OnLobbyLeft(LobbyLeftMessage ev) 
    {
        _stateManager.RequestStateChange<LobbyListState>();
    }

    private void StateManagerOnOnStateChanged(StateChangedEventArgs obj)
    {
        if (_cameraUid.HasValue && obj.NewState is not UserInterface.States.GameState)
        {
            EntityManager.DeleteEntity(_cameraUid.Value);
        }

        if(obj.OldState is IBeachBallState oldState) oldState.UnsubscribeFromEvents(this);
        
        if (obj.NewState is not IBeachBallState beachBallState) return;
        
        beachBallState.SubscribeToEvents(this);
        beachBallState.UpdateData(this);
    }

    private void OnPlayerAttached(PlayerAttachSysMessage ev)
    {
        if (!ev.AttachedEntity.Valid)
            return;

        // This will set a camera in the middle of the arena.
        _cameraUid = EntityManager.SpawnEntity(null, new MapCoordinates(new Vector2(0,0), Transform(ev.AttachedEntity).MapID));
        var eye = EnsureComp<EyeComponent>(_cameraUid.Value);
        eye.Current = true;
        eye.Zoom = Vector2.One * 1.5f;
    }

    private void OnScored(ScoreUpdate ev)
    {
        if (GameState == null) return;
        
        GameState.Score = ev.Scores;
        (_stateManager.CurrentState as IBeachBallState)?.UpdateData(this);
    }

    private void OnGameCreated(GameCreatedMessage ev)
    {
        GameState = ev.Game;
        _stateManager.RequestStateChange<GameState>();
    }

    private void OnLobbyChanged(LobbyListMessage ev)
    {
        Lobbies = ev.Lobbies;
        (_stateManager.CurrentState as IBeachBallState)?.UpdateData(this);
    }

    public void Disconnect()
    {
        _clientNetManager.ClientDisconnect("Disconnected");
    }

    public void JoinLobby(string lobby, string? pw = null)
    {
        RaiseNetworkEvent(new JoinLobbyRequestMessage(){Name = lobby, Password = pw});
    }
    
    public void CreateLobby(string name, string? pw)
    {
        RaiseNetworkEvent(new CreateLobbyRequestMessage(){Name = name, Password = pw});
    }

    public void LeaveLobby()
    {
        RaiseNetworkEvent(new LeaveLobbyRequestMessage());
    }

    public void StartLobby()
    {
        RaiseNetworkEvent(new StartLobbyRequestMessage(){Name = selectedLobby});
    }
}