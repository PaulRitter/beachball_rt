using System;
using System.Collections.Generic;
using Content.Shared.Ball;
using Content.Shared.Paddle;
using Robust.Shared.Analyzers;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared;

[Virtual]
public abstract class SharedBeachballSystem : EntitySystem
{
    [Dependency] private readonly IPhysicsManager _physicsManager = default!;

    public const float BoostCoolDown = 2f;
    public const float MaxTimeBetweenBoostPress = 1f;   
    public const int PlayerSpeed = 500;
    public const int JumpSpeed = 2000;
    public const float MaxHorizontalVelocity = 10;
    public const string FloorFixtureId = "Floor";
    public static Vector2 P1Coordinates = new(-15, -10);
    public static Vector2 P2Coordinates = new(15, -10);
    public static Vector2 P1BallCoordinates = new(-13, -3);
    public static Vector2 P2BallCoordinates = new(13, -3);
    public const int WinScore = 2;
    public static TimeSpan AfterWinDuration = TimeSpan.FromSeconds(1);

    //todo implement to predict resetting on client
    public virtual void OnScored(MapId mapId, int ballIndex){}
    protected void ResetField(List<EntityUid> players, EntityUid ball, int ballIndex)
    {
        Transform(ball).WorldPosition = ballIndex == 0 ? P1BallCoordinates : P2BallCoordinates;
        Comp<BallComponent>(ball).Frozen = true;

        Transform(players[0]).WorldPosition = P1Coordinates;
        Transform(players[1]).WorldPosition = P2Coordinates;
        _physicsManager.ClearTransforms();
    }
}

//todo make this less shit
[Serializable, NetSerializable]
public sealed class LobbyListMessage : EntityEventArgs
{
    public List<NetworkedLobby> Lobbies { get; init; }
}

[Serializable, NetSerializable]
public sealed class GameCreatedMessage : EntityEventArgs
{
    public NetworkedBeachballGame Game { get; init; }
}

[Serializable, NetSerializable]
public sealed class ScoreUpdate : EntityEventArgs
{
    public List<int> Scores { get; init; }
}

[Serializable, NetSerializable]
public sealed class NetworkedLobby
{
    public string Name;
    public Dictionary<string, bool> Players;
    public bool Password;

    public NetworkedLobby(string name, Dictionary<string, bool> players, bool password)
    {
        Name = name;
        Players = players;
        Password = password;
    }
}

[Serializable, NetSerializable]
public sealed class NetworkedBeachballGame
{
    public string Name { get; init; }
    public List<string> Players { get; init; }
    public List<int> Score { get; set; }
}

[Serializable, NetSerializable]
public enum BeachballPlayerState
{
    MainMenu,
    Lobby,
    Game
}