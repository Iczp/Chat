using System;
using System.ComponentModel.DataAnnotations;
using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.ChatObjectTypes.Dtos;

public class ChatObjectTypeAdminGetListInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }
}

public class ChatObjectTypeAdminDto : EntityDto<string>
{
    public string Name { get; set; }

    public ChatObjectTypeEnums ObjectType { get; set; }

    public string Description { get; set; }

    public int MaxDepth { get; set; }

    public bool IsHasChild { get; set; }

    public bool IsStatic { get; set; }

    public int ChatObjectCount { get; set; }

    public DateTime CreationTime { get; set; }

    public DateTime? LastModificationTime { get; set; }
}

public class ChatObjectTypeAdminCreateInput
{
    [Required]
    [MaxLength(64)]
    public string Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    public ChatObjectTypeEnums? ObjectType { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    public int MaxDepth { get; set; }

    public bool IsHasChild { get; set; }
}

public class ChatObjectTypeAdminUpdateInput
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    public ChatObjectTypeEnums? ObjectType { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    public int MaxDepth { get; set; }

    public bool IsHasChild { get; set; }
}
