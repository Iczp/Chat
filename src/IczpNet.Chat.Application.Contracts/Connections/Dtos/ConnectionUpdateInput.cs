using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Connections.Dtos;

public class ConnectionUpdateInput : BaseInput
{
    public virtual DateTime ActiveTime { get; set; }
}
