using System.Collections.Generic;
using System.Linq;
using Content.Shared;
using Robust.Server.Player;

namespace Content.Server;

public sealed class BeachBallGame
{
    public string Name { get; init; }
    public List<IPlayerSession> Players { get; init; }
    public List<int> Score { get; init; }

    public static implicit operator GameCreatedMessage(BeachBallGame game) => new()
    {
        Game = new NetworkedBeachballGame
            { Players = game.Players.Select(x => x.Name).ToList(), Name = game.Name, Score = game.Score }
    };
}