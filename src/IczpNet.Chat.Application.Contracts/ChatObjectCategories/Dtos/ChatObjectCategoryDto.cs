using System;
using Volo.Abp.Application.Dtos;
namespace IczpNet.Chat.ChatObjectCategories.Dtos;

/// <summary>
/// ChatObjectCategory Dto
/// </summary>
public class ChatObjectCategoryDto : ChatObjectCategoryInfo, IEntityDto<Guid>
{

    public virtual string Description { get; set; }
}
