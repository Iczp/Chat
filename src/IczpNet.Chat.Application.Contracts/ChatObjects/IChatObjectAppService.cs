using IczpNet.Chat.ChatObjects.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.ChatObjects
{
    public interface IChatObjectAppService :
        ICrudAppService<
            ChatObjectDetailDto,
            ChatObjectDto,
            Guid,
            ChatObjectGetListInput,
            ChatObjectCreateInput,
            ChatObjectUpdateInput>
    {
    }
}
