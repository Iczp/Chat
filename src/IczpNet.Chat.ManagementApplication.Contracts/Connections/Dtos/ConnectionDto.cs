using IczpNet.Chat.Management.BaseDtos;
using System;

namespace IczpNet.Chat.Management.Management.Connections.Dtos;

public class ConnectionDto : BaseDto<Guid>
{
    public virtual Guid? AppUserId { get; set; }

    public virtual Guid? ChatObjectId { get; set; }

    public virtual string Server { get; set; }

    public virtual string DeviceId { get; set; }

    public virtual string Ip { get; set; }

    public virtual string Agent { get; set; }

    public virtual DateTime ActiveTime { get; set; } 

}
