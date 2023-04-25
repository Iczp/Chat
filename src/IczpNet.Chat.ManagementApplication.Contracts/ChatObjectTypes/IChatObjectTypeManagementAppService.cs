using IczpNet.Chat.Management.ChatObjectTypes.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Management.ChatObjectTypes;

public interface IChatObjectTypeManagementAppService :
    ICrudAppService<
        ChatObjectTypeDetailDto,
        ChatObjectTypeDto,
        string,
        ChatObjectTypeGetListInput,
        ChatObjectTypeCreateInput,
        ChatObjectTypeUpdateInput>
{
}
