using System.Collections.Generic;
using System.Linq;
using Content.Shared;
using Robust.Server.Player;
using Robust.Shared.Players;

namespace Content.Server;

public sealed class Lobby
{
    public string Name;
    public string? Password;
    public Dictionary<IPlayerSession, bool> Players = new();

    public Lobby(string name, string? password)
    {
        Name = name;
        Password = password;
    }

    public void AddPlayer(IPlayerSession session)
    {
        Players[session] = false;
    }

    public void MakeAdmin(IPlayerSession session)
    {
        if (Players.ContainsKey(session))
            Players[session] = true;
    }

    public void RemovePlayer(IPlayerSession session)
    {
        Players.Remove(session);
    }

    public static implicit operator NetworkedLobby(Lobby lobby) => new(lobby.Name, lobby.Players.ToDictionary(x => x.Key.Name, x => x.Value), lobby.Password != null); }