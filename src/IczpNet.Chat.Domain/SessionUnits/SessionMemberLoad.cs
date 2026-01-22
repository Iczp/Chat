using System;

namespace IczpNet.Chat.SessionUnits;

[Flags]
public enum SessionMemberLoad
{
    None = 0,
    Pinned = 1 << 0,
    Immersed = 1 << 1,
    Creator = 1 << 2,
    Private = 1 << 3,
    Static = 1 << 4,
    Box = 1 << 5,
}