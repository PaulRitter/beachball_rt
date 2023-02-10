using System;
using System.Collections.Generic;
using System.Linq;
using Content.Shared;
using Content.Shared.Ball;
using Content.Shared.ClientMessages;
using Content.Shared.ServerMessages;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Players;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server;

public sealed class BeachBallSystem : SharedBeachballSystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IMapLoader _mapLoader = default!;

    private Dictionary<ICommonSession, MapId> _activePlayers = new();
    private Dictionary<ICommonSession, BeachballPlayerState> _playerGameStates = new();
    private Dictionary<string, Lobby> _waitingLobbies = new();
    private Dictionary<MapId, BeachBallGame> _gameStates = new();
    
    public override void Initialize()
    {
        base.Initialize();
        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;
        SubscribeNetworkEvent<CreateLobbyRequestMessage>(OnCreateLobbyRequest);
        SubscribeNetworkEvent<StartLobbyRequestMessage>(OnStartLobbyRequest);
        SubscribeNetworkEvent<JoinLobbyRequestMessage>(OnJoinLobbyRequest);
        SubscribeNetworkEvent<LeaveLobbyRequestMessage>(OnLeaveLobbyRequest);
    }

    private void OnJoinLobbyRequest(JoinLobbyRequestMessage ev, EntitySessionEventArgs args)
    {
        JoinLobby((IPlayerSession)args.SenderSession, ev.Name, ev.Password);
    }

    private void OnLeaveLobbyRequest(LeaveLobbyRequestMessage ev, EntitySessionEventArgs args)
    {
        LeaveLobby((IPlayerSession)args.SenderSession);
    }

    private void OnStartLobbyRequest(StartLobbyRequestMessage msg, EntitySessionEventArgs args)
    {
        if (!_waitingLobbies.TryGetValue(msg.Name, out var lobby) ||
            !lobby.Players.TryGetValue((IPlayerSession)args.SenderSession, out var isAdmin) ||
            !isAdmin ||
            lobby.Players.Count < 2) 
            return;
        
        var gameMap = _mapManager.CreateMap();

        var players = lobby.Players.Keys.ToList();
        var playerUids = new List<EntityUid>();
        var p1 = EntityManager.SpawnEntity("playerMonkey", new MapCoordinates(0, 0, gameMap));
        players[0].AttachToEntity(p1);
        playerUids.Add(p1);

        var p2 = EntityManager.SpawnEntity("playerCat", new MapCoordinates(0, 0, gameMap));
        players[1].AttachToEntity(p2);
        playerUids.Add(p2);

        if (players.Count > 2)
        {
            for (int i = 2; i < players.Count; i++)
            {
                var entity = EntityManager.SpawnEntity(null, new MapCoordinates(0, 0, gameMap));
                players[i].AttachToEntity(entity);
            }
        }

        EntityManager.SpawnEntity("arenaNet", new MapCoordinates(0, -10, gameMap));
        EntityManager.SpawnEntity("arenaBackground", new MapCoordinates(0, 0, gameMap));

        var ball = EntityManager.SpawnEntity("ballBeach", new MapCoordinates(0, 0, gameMap));

        _waitingLobbies.Remove(msg.Name);
        var game = new BeachBallGame()
        {
            Score = new List<int>(),
            PlayerUids = playerUids,
            BallUid = ball,
            Lobby = lobby
        };
        game.Score.Add(0);
        game.Score.Add(0);
        _gameStates.Add(gameMap, game);
        RaiseNetworkEvent(game);
        ResetField(game.PlayerUids, game.BallUid, 0);
    }

    private void OnCreateLobbyRequest(CreateLobbyRequestMessage ev, EntitySessionEventArgs args)
    {
        // remove excess whitespaces
        var name = ev.Name.Trim();
        //TODO: notify user
        if(string.IsNullOrWhiteSpace(name) ||
            _waitingLobbies.ContainsKey(name) ||
            _gameStates.Any(x => x.Value.Lobby.Name == name) ||
            _playerGameStates[args.SenderSession] != BeachballPlayerState.MainMenu)
            return;

        var password = string.IsNullOrWhiteSpace(ev.Password) ? null : ev.Password;

        var lobby = new Lobby(name, password);
        _waitingLobbies[name] = lobby;
        JoinLobby((IPlayerSession)args.SenderSession, name, password);
        lobby.MakeAdmin((IPlayerSession)args.SenderSession);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _playerManager.PlayerStatusChanged -= OnPlayerStatusChanged;
    }

    private void SetPlayerState(ICommonSession session, BeachballPlayerState state)
    {
        _playerGameStates[session] = state;
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus == SessionStatus.Connected)
        {
            e.Session.JoinGame();
            SetPlayerState(e.Session, BeachballPlayerState.MainMenu);
            RaiseNetworkEvent(new LobbyListMessage(){Lobbies = _waitingLobbies.Values.Select(x => (NetworkedLobby)x).ToList()});
        }
    }

    private void JoinLobby(IPlayerSession session, string name, string? pw)
    {
        if(_playerGameStates[session] != BeachballPlayerState.MainMenu)
            return;
        
        if(!_waitingLobbies.TryGetValue(name, out var sharedLobby))
            return;

        var lobby = sharedLobby;

        if (lobby.Password != null && lobby.Password != pw)
            return;
        
        lobby.AddPlayer(session);
        SetPlayerState(session, BeachballPlayerState.Lobby);
        RaiseNetworkEvent(new LobbyListMessage(){Lobbies = _waitingLobbies.Values.Select(x => (NetworkedLobby)x).ToList()});
        RaiseNetworkEvent(new LobbyJoinedMessage(){Name = name}, session.ConnectedClient);
    }

    private void LeaveLobby(IPlayerSession session)
    {
        if(_playerGameStates[session] != BeachballPlayerState.Lobby)
            return;

        //TODO: unshit this
        if (_waitingLobbies.TryFirstOrNull(x => x.Value.Players.ContainsKey(session), out var val))
        {
            val.Value.Value.RemovePlayer(session);
            if (val.Value.Value.Players.Count == 0)
            {
                // Remove the lobby from the list
                _waitingLobbies.Remove(val.Value.Key);
            }
            else
            {
                //TODO: Make someone else admin
            }
            RaiseNetworkEvent(new LobbyLeftMessage(), session.ConnectedClient);
            RaiseNetworkEvent(new LobbyListMessage(){Lobbies = _waitingLobbies.Values.Select(x => (NetworkedLobby)x).ToList()});
        }
        
        SetPlayerState(session, BeachballPlayerState.MainMenu);
    }

    public override void OnScored(MapId mapId, int ballIndex)
    {
        if(!_gameStates.TryGetValue(mapId, out var game))
            return;

        game.Score[ballIndex] += 1;
        RaiseNetworkEvent(new ScoreUpdate(){Scores = game.Score});

        if (game.Score[ballIndex] >= WinScore)
        {
            //win!!!!!
            _mapManager.SetMapPaused(mapId, true);
            Timer.Spawn(AfterWinDuration, () => CloseGame(mapId));
            return;
        }
        
        ResetField(game.PlayerUids, game.BallUid, ballIndex);
    }

    private void CloseGame(MapId mapId)
    {
        var game = _gameStates[mapId];
        _gameStates.Remove(mapId);
        _mapManager.DeleteMap(mapId);
        
        var lobby = game.Lobby;
        _waitingLobbies[lobby.Name] = lobby;
        
        foreach (var (player, _) in lobby.Players)
        {
            RaiseNetworkEvent(new LobbyJoinedMessage(){Name = lobby.Name}, player.ConnectedClient);
        }
        RaiseNetworkEvent(new LobbyListMessage(){Lobbies = _waitingLobbies.Values.Select(x => (NetworkedLobby)x).ToList()});
    }
}