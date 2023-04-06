using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
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
        ITreeAppService<
            ChatObjectDetailDto,
            ChatObjectDto,
            long,
            ChatObjectGetListInput,
            ChatObjectCreateInput,
            ChatObjectUpdateInput, ChatObjectInfo>
    {
        Task<PagedResultDto<ChatObjectDto>> GetListByUserIdAsync(Guid userId, int maxResultCount = 10, int skipCount = 0, string sorting = null);

        Task<PagedResultDto<ChatObjectDto>> GetListByCurrentUserAsync(int maxResultCount = 10, int skipCount = 0, string sorting = null);

        Task<ChatObjectDetailDto> GetByCodeAsync(string code);

        Task<ChatObjectDto> CreateShopKeeperAsync(string name);

        Task<ChatObjectDto> CreateShopWaiterAsync(long shopKeeperId, string name);

    }
}
