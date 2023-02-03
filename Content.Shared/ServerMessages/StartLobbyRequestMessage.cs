using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.ServerMessages;

[NetSerializable, Serializable]
public sealed class StartLobbyRequestMessage : EntityEventArgs
{
    public string Name { get; init; }
}