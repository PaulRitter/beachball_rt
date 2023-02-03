using System;
using System.Collections.Generic;
using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared;

[Virtual]
public class SharedBeachballSystem : EntitySystem
{
    public static readonly (int left, int right) FieldBounds = (0,100);
    public static readonly uint DoubleTickDelay = 100;
    public static readonly uint DoubleTickDuration = 50;
    public static readonly int PlayerSpeed = 6;
}

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