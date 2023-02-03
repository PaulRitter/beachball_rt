using System;
using System.Collections.Generic;
using Content.Shared.Paddle;
using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Serialization;

namespace Content.Shared;

[Virtual]
public abstract class SharedBeachballSystem : EntitySystem
{
    public static readonly (int left, int right) FieldBounds = (-50,50);
    public const float BoostCoolDown = 2f;
    public const float MaxTimeBetweenBoostPress = 1f;   
    public const int PlayerSpeed = 500;
    public const int JumpSpeed = 2000;
    public const float MaxHorizontalVelocity = 10;
    public const string FloorFixtureId = "Floor";

    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<BeacherComponent, StartCollideEvent>(OnBeacherStartCollide);
        SubscribeLocalEvent<BeacherComponent, EndCollideEvent>(OnBeacherEndCollide);
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
public sealed class ScoredMessage : EntityEventArgs
{
    public int Index;
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
    public List<int> Score { get; init; }
}

[Serializable, NetSerializable]
public enum BeachballPlayerState
{
    MainMenu,
    Lobby,
    Game
}