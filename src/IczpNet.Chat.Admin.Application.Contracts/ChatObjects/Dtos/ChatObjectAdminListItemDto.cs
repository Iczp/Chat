using System;
using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.ChatObjects.Dtos;

public class ChatObjectAdminListItemDto : EntityDto<long>
{
    public string Name { get; set; }

    public string DisplayName { get; set; }

    public string Code { get; set; }

    public Guid? AppUserId { get; set; }

    public string ChatObjectTypeId { get; set; }

    public ChatObjectTypeEnums? ObjectType { get; set; }

    public string ObjectTypeDescription { get; set; }

    public Genders Gender { get; set; }

    public VerificationMethods VerificationMethod { get; set; }

    public string Thumbnail { get; set; }

    public string Portrait { get; set; }

    public string Description { get; set; }

    public long? ParentId { get; set; }

    public int Depth { get; set; }

    public string FullPath { get; set; }

    public string FullPathName { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsDefault { get; set; }

    public bool IsPublic { get; set; }

    public bool IsStatic { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeveloper { get; set; }

    public DateTime CreationTime { get; set; }
}
