using IczpNet.Chat.Management.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.Management.Connections.Dtos;

public class ConnectionUpdateInput : BaseInput
{



    public virtual DateTime ActiveTime { get; set; }
}
