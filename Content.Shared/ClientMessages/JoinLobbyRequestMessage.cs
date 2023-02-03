using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.ClientMessages;

[Serializable, NetSerializable]
public sealed class JoinLobbyRequestMessage : EntityEventArgs
{
    public string Name { get; init; }
}