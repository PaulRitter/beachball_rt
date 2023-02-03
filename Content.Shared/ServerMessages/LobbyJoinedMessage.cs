using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.ServerMessages;

[Serializable, NetSerializable]
public sealed class LobbyJoinedMessage : EntityEventArgs
{
    public string Name { get; init; }
}