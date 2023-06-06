using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.EntryNames.Dtos;
using IczpNet.Chat.EntryValues.Dtos;
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.ChatObjects
{
    public interface IChatObjectAppService :
        ICrudAppService<
            ChatObjectDto,
            ChatObjectDto,
            long,
            ChatObjectGetListInput,
            ChatObjectCreateInput,
            ChatObjectUpdateInput>
        ,
        ITreeAppService<
            ChatObjectDto,
            ChatObjectDto,
            long,
            ChatObjectGetListInput,
            ChatObjectCreateInput,
            ChatObjectUpdateInput, ChatObjectInfo>
    {
        Task<PagedResultDto<ChatObjectDto>> GetListByUserIdAsync(Guid userId, int maxResultCount = 10, int skipCount = 0, string sorting = null);

        Task<PagedResultDto<ChatObjectDto>> GetListByCurrentUserAsync(int maxResultCount = 10, int skipCount = 0, string sorting = null);

        Task<ChatObjectDto> GetByCodeAsync(string code);

        Task<ChatObjectDto> CreateShopKeeperAsync(string name);

        Task<ChatObjectDto> CreateShopWaiterAsync(long shopKeeperId, string name);

        Task<ChatObjectDto> UpdateNameAsync(long id, string name);

        Task<ChatObjectDto> UpdatePortraitAsync(long id, string portrait);

        Task<ChatObjectDto> SetVerificationMethodAsync(long id, VerificationMethods verificationMethod);

        Task<ChatObjectDetailDto> GetDetailAsync(long id);

        Task<ChatObjectDetailDto> SetEntriesAsync(long id, Dictionary<Guid, List<EntryValueInput>> input);

    }
}
