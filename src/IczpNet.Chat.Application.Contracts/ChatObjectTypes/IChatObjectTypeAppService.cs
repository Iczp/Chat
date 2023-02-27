using IczpNet.Chat.ChatObjectTypes.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.ChatObjectTypes;

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
