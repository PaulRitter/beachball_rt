using System.Collections.Generic;
using System.Linq;
using Content.Shared;
using Robust.Server.Player;
using Robust.Shared.GameObjects;

namespace Content.Server;

public sealed class BeachBallGame
{
    public List<EntityUid> PlayerUids { get; init; }
    public EntityUid BallUid { get; init; }
    public List<int> Score { get; init; }
    public Lobby Lobby { get; init; }

    public static implicit operator GameCreatedMessage(BeachBallGame game) => new()
    {
        Game = new NetworkedBeachballGame
            { Players = game.Lobby.Players.Select(x => x.Key.Name).ToList(), Name = game.Lobby.Name, Score = game.Score }
    };
}