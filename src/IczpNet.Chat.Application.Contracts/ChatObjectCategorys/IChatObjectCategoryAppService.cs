using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjectCategorys.Dtos;
using System;

namespace IczpNet.Chat.ChatObjectCategorys;

/// <summary>
/// 聊天对象目录（分组）
/// </summary>
public interface IChatObjectCategoryAppService :
    ITreeAppService<ChatObjectCategoryDetailDto,
        ChatObjectCategoryDto,
        Guid,
        ChatObjectCategoryGetListInput,
        ChatObjectCategoryCreateInput,
        ChatObjectCategoryUpdateInput, 
        ChatObjectCategoryInfo>
{
}
