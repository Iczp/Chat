using System;
using System.Collections.Generic;
using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.ChatObjects.Dtos;

public class ChatObjectAdminGetListInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }

    public string Code { get; set; }

    public Guid? AppUserId { get; set; }

    public string ChatObjectTypeId { get; set; }

    public ChatObjectTypeEnums? ObjectType { get; set; }

    public List<ChatObjectTypeEnums> ObjectTypes { get; set; }

    public List<Guid> CategoryIdList { get; set; }

    public bool IncludeChildCategories { get; set; }

    public bool? IsEnabled { get; set; }

    public bool? IsDefault { get; set; }

    public bool? IsPublic { get; set; }

    public bool? IsStatic { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeveloper { get; set; }
}
