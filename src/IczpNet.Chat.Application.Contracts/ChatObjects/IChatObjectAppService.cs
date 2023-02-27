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
            Guid,
            ChatObjectGetListInput,
            ChatObjectCreateInput,
            ChatObjectUpdateInput>
        ,
        ITreeAppService<Guid, ChatObjectInfo>
    {
        Task<PagedResultDto<ChatObjectDto>> GetListByUserIdAsync(Guid userId, int maxResultCount = 10, int skipCount = 0, string sorting = null);
        Task<ChatObjectDetailDto> GetByAutoIdAsync(long autoId);
        Task<ChatObjectDetailDto> GetByCodeAsync(string code);

    }
}
