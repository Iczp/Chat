using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Connections.Dtos;

public class ConnectionDto : BaseDto<string>
{
    public virtual string ServerHostId { get; set; }

    public virtual Guid? AppUserId { get; set; }

    public virtual string DeviceId { get; set; }

    public virtual string IpAddress { get; set; }

    public virtual string ChatObjects { get; set; }

    public virtual DateTime ActiveTime { get; set; }

}
