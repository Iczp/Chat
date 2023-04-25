using IczpNet.Chat.Management.BaseDtos;
using System;

namespace IczpNet.Chat.Management.Connections.Dtos;

public class ConnectionCreateInput : BaseInput
{
    public virtual Guid? AppUserId { get; set; }

    public virtual Guid? ChatObjectId { get; set; }

    public virtual string Server { get; set; }

    public virtual string DeviceId { get; set; }

    public virtual string Ip { get; set; }

    public virtual string Agent { get; set; }
}
