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
using Robust.Shared.Players;
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

    private HashSet<MapId> _processed = new();

    public override void Initialize()
    {
        base.Initialize();
        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;
        SubscribeNetworkEvent<CreateLobbyRequestMessage>(OnCreateLobbyRequest);
        SubscribeNetworkEvent<StartLobbyRequestMessage>(OnStartLobbyRequest);
        SubscribeNetworkEvent<JoinLobbyRequestMessage>(OnJoinLobbyRequest);
    }

    private void OnJoinLobbyRequest(JoinLobbyRequestMessage ev, EntitySessionEventArgs args)
    {
        JoinLobby((IPlayerSession)args.SenderSession, ev.Name, null);
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
        var p1 = EntityManager.SpawnEntity("playerMonkey", new MapCoordinates(-15, -10, gameMap));
        players[0].AttachToEntity(p1);
        playerUids.Add(p1);

        var p2 = EntityManager.SpawnEntity("playerCat", new MapCoordinates(15, -10, gameMap));
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

        var ball = EntityManager.SpawnEntity("ballBeach", new MapCoordinates(-13, -3, gameMap));

        _waitingLobbies.Remove(msg.Name);
        var game = new BeachBallGame()
        {
            Name = msg.Name,
            Players = players,
            Score = new List<int>(),
            PlayerUids = playerUids,
            BallUid = ball
        };
        game.Score.Add(0);
        game.Score.Add(0);
        _gameStates.Add(gameMap, game);
        RaiseNetworkEvent(game);
        StartRound(gameMap, 0);
    }

    private void OnCreateLobbyRequest(CreateLobbyRequestMessage ev, EntitySessionEventArgs args)
    {
        if(_waitingLobbies.ContainsKey(ev.Name) || _playerGameStates[args.SenderSession] != BeachballPlayerState.MainMenu)
            return;

        var lobby = new Lobby(ev.Name, null);
        _waitingLobbies[ev.Name] = lobby;
        JoinLobby((IPlayerSession)args.SenderSession, ev.Name, null);
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

        if (_waitingLobbies.TryFirstOrNull(x => x.Value.Players.ContainsKey(session), out var val))
        {
            val.Value.Value.RemovePlayer(session);
            RaiseNetworkEvent(new LobbyListMessage(){Lobbies = _waitingLobbies.Values.Select(x => (NetworkedLobby)x).ToList()});
        }
        
        SetPlayerState(session, BeachballPlayerState.MainMenu);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _processed.Clear();
        foreach (var (_, transform) in EntityManager.EntityQuery<BallComponent, TransformComponent>())
        {
            if(_processed.Contains(transform.MapID) || Paused(_mapManager.GetMapEntityId(transform.MapID)))
                continue;

            _processed.Add(transform.MapID);
            
            if (transform.MapPosition.Y <= 0)
            {
                var index = 0;
                //someone scored
                if (transform.MapPosition.X > FieldBounds.right / 2f)
                {
                    //p1
                    index = 0;
                }
                else
                {
                    //p2
                    index = 1;
                }

                var game = _gameStates[transform.MapID];
                game.Score[index] += 1;
                RaiseNetworkEvent(new ScoredMessage(){Index = index});

                //todo check for win condition

                index++;
                if (index > game.Players.Count - 1)
                {
                    index = 0;
                }
                StartRound(transform.MapID, index);
            }
        }
    }

    private void StartRound(MapId mapId, int ballIndex)
    {
        //todo players to positions
        //todo ball to position
        //todo freeze ball until player moves
    }
}