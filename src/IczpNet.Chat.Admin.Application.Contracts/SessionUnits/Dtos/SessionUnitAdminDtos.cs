using System;
using IczpNet.Chat.Admin.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.SessionUnits.Dtos;

public class SessionUnitAdminGetListInput : PagedAndSortedResultRequestDto
{
    public long? OwnerId { get; set; }

    public long? DestinationId { get; set; }

    public ChatObjectTypeEnums? DestinationObjectType { get; set; }

    public bool? IsKilled { get; set; }

    public string Keyword { get; set; }
}

public class SessionUnitAdminSettingDto
{
    public bool IsEnabled { get; set; }

    public bool IsInputEnabled { get; set; }

    public bool IsKilled { get; set; }
}

public class SessionUnitAdminDto : EntityDto<Guid>
{
    public long OwnerId { get; set; }

    public long? DestinationId { get; set; }

    public ChatObjectTypeEnums? DestinationObjectType { get; set; }

    public Guid? SessionId { get; set; }

    public ChatObjectAdminListItemDto Destination { get; set; }

    public SessionUnitAdminSettingDto Setting { get; set; }

    public long? LastMessageId { get; set; }

    public int PublicBadge { get; set; }

    public int PrivateBadge { get; set; }

    public DateTime CreationTime { get; set; }
}
