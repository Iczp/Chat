using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.Online.Dtos;

public class OnlineAdminGetListInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }

    public string Host { get; set; }

    public string ConnectionId { get; set; }

    public string ClientId { get; set; }

    public string Platform { get; set; }

    public Guid? UserId { get; set; }

    public long? ChatObjectId { get; set; }

    public DateTime? StartActiveTime { get; set; }

    public DateTime? EndActiveTime { get; set; }

    public DateTime? StartCreationTime { get; set; }

    public DateTime? EndCreationTime { get; set; }
}

public class OnlineAdminHostGetListInput : PagedAndSortedResultRequestDto
{
    public string Host { get; set; }
}

public class OnlineAdminLastOnlineGetListInput : PagedAndSortedResultRequestDto
{
    public string DeviceId { get; set; }

    public string DeviceType { get; set; }
}

public class OnlineAdminConnectionDto
{
    public string ConnectionId { get; set; }

    public string PushClientId { get; set; }

    public string AppId { get; set; }

    public string AppName { get; set; }

    public string QueryId { get; set; }

    public string ClientId { get; set; }

    public string ClientName { get; set; }

    public string Host { get; set; }

    public Guid? UserId { get; set; }

    public string UserName { get; set; }

    public string DeviceId { get; set; }

    public string DeviceType { get; set; }

    public string IpAddress { get; set; }

    public string BrowserInfo { get; set; }

    public string DeviceInfo { get; set; }

    public DateTime? ActiveTime { get; set; }

    public DateTime CreationTime { get; set; }

    public string Browser { get; set; }

    public string Brand { get; set; }

    public string Model { get; set; }

    public string Platform { get; set; }

    public List<long> ChatObjectIdList { get; set; } = new();
}

public class OnlineAdminHostDto
{
    public string Host { get; set; }

    public DateTime? StartTime { get; set; }

    public long Count { get; set; }
}

public class OnlineAdminLastOnlineDto
{
    public long OwnerId { get; set; }

    public string DeviceType { get; set; }

    public string DeviceId { get; set; }

    public DateTime? ActiveTime { get; set; }
}

public class OnlineAdminAbortInput
{
    [Required]
    public List<string> ConnectionIdList { get; set; } = new();

    public string Reason { get; set; }
}
