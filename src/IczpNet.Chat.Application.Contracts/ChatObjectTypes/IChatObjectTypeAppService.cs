using IczpNet.Chat.ChatObjectTypes.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.ChatObjectTypes;

/// <summary>
/// 聊天对象类型
/// </summary>
public interface IChatObjectTypeAppService :
    ICrudAppService<
        ChatObjectTypeDetailDto,
        ChatObjectTypeDto,
        string,
        ChatObjectTypeGetListInput,
        ChatObjectTypeCreateInput,
        ChatObjectTypeUpdateInput>
{
}
