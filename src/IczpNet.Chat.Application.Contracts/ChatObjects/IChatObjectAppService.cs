using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjects.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.ChatObjects
{
    public interface IChatObjectAppService :
        ICrudAppService<
            ChatObjectDetailDto,
            ChatObjectDto,
            long,
            ChatObjectGetListInput,
            ChatObjectCreateInput,
            ChatObjectUpdateInput>
        ,
        ITreeAppService<long, ChatObjectInfo>
    {
        Task<PagedResultDto<ChatObjectDto>> GetListByUserIdAsync(Guid userId, int maxResultCount = 10, int skipCount = 0, string sorting = null);
        Task<ChatObjectDetailDto> GetByCodeAsync(string code);

    }
}
