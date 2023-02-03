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
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
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
    public const int WinScore = 11;
    public static TimeSpan AfterWinDuration = TimeSpan.FromSeconds(1);

    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<BeacherComponent, StartCollideEvent>(OnBeacherStartCollide);
        SubscribeLocalEvent<BeacherComponent, EndCollideEvent>(OnBeacherEndCollide);
        SubscribeLocalEvent<BallComponent, StartCollideEvent>(OnBallCollide);
    }
    
    //todo implement to predict resetting on client
    protected virtual void OnScored(MapId mapId, int ballIndex){}
    protected void ResetField(List<EntityUid> players, EntityUid ball, int ballIndex)
    {
        Transform(ball).WorldPosition = ballIndex == 0 ? P1BallCoordinates : P2BallCoordinates;
        Comp<BallComponent>(ball).Frozen = true;

        Transform(players[0]).WorldPosition = P1Coordinates;
        Transform(players[1]).WorldPosition = P2Coordinates;
        _physicsManager.ClearTransforms();
    }    
    private void OnBallCollide(EntityUid uid, BallComponent component, StartCollideEvent args)
    {
        if (args.OtherFixture.ID == "Floor")
        {
            var transform = Transform(uid);

            var scoredIndex = transform.WorldPosition.X < 0 ? 1 : 0;
            OnScored(transform.MapID, scoredIndex);
        }
        
        if (args.OtherFixture.ID == "Player")
        {
            Comp<BallComponent>(uid).Frozen = false;
            _audioSystem.PlayGlobal("/Audio/bloop.wav", Filter.Broadcast(), AudioParams.Default.WithVolume(-5f));
        }
    }

    private void OnBeacherStartCollide(EntityUid uid, BeacherComponent component, StartCollideEvent args)
    {
        if(args.OtherFixture.ID != FloorFixtureId) return;

        component.TouchingFloor = true;
        component.CanBoost = true;
    }
    
    private void OnBeacherEndCollide(EntityUid uid, BeacherComponent component, EndCollideEvent args)
    {
        if(args.OtherFixture.ID != FloorFixtureId) return;
        
        component.TouchingFloor = false;
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