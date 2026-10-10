using IczpNet.Chat.ChatObjectCategories;
using System;
using Volo.Abp.Application.Dtos;
namespace IczpNet.Chat.Management.ChatObjectCategorys.Dtos;

/// <summary>
/// ChatObjectCategory Dto
/// </summary>
public class ChatObjectCategoryDto : ChatObjectCategoryInfo, IEntityDto<Guid>
{

    public virtual string Description { get; set; }

    public virtual string GetObjectKey() => $"{Id}";
}
