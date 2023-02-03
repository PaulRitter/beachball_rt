using System.Collections.Generic;
using Content.Client.UserInterface.States;
using Content.Shared;
using Content.Shared.ClientMessages;
using Content.Shared.ServerMessages;
using Robust.Client.GameObjects;
using Robust.Client.State;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.Client;

public sealed class BeachballSystem : SharedBeachballSystem
{
    [Dependency] private readonly IStateManager _stateManager = default!;
    
    public List<NetworkedLobby> Lobbies = new();
    public string? selectedLobby;
    public NetworkedBeachballGame? GameState;
    public BeachballPlayerState PlayerState;
    
    public override void Initialize()
    {
        base.Initialize();
            
        SubscribeNetworkEvent<LobbyListMessage>(OnLobbyChanged);
        SubscribeNetworkEvent<GameCreatedMessage>(OnGameCreated);
        SubscribeNetworkEvent<ScoredMessage>(OnScored);
        SubscribeNetworkEvent<LobbyJoinedMessage>(OnLobbyJoined);
        
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

    private void StateManagerOnOnStateChanged(StateChangedEventArgs obj)
    {
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
        var camera = EntityManager.SpawnEntity(null, new MapCoordinates(new Vector2(0,0), Transform(ev.AttachedEntity).MapID));
        var eye = EnsureComp<EyeComponent>(camera);
        eye.Current = true;
        eye.Zoom = Vector2.One * 1.5f;
    }

    private void OnScored(ScoredMessage ev)
    {
        if (GameState == null) return;
        
        GameState.Score[ev.Index] += 1;
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

    public void JoinLobby(string lobby)
    {
        RaiseNetworkEvent(new JoinLobbyRequestMessage(){Name = lobby});
    }
    
    public void CreateLobby(string name)
    {
        RaiseNetworkEvent(new CreateLobbyRequestMessage(){Name = name});
    }

    public void StartLobby()
    {
        RaiseNetworkEvent(new StartLobbyRequestMessage(){Name = selectedLobby});
    }
}