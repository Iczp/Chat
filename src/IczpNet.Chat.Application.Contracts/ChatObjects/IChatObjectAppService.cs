using IczpNet.AbpTrees;
using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.ChatObjects
{
    /// <summary>
    /// 聊天对象
    /// </summary>
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
        Task<PagedResultDto<ChatObjectDto>> GetListByUserIdAsync(Guid userId, BaseGetListInput input);

        Task<PagedResultDto<ChatObjectDto>> GetListByCurrentUserAsync(BaseGetListInput input);

        Task<ChatObjectDto> GetByCodeAsync(string code);

        Task<ChatObjectDto> CreateShopKeeperAsync(string name);

        Task<ChatObjectDto> CreateShopWaiterAsync(long shopKeeperId, string name);

        Task<ChatObjectDto> UpdateNameAsync(long id, string name);

        /// <summary>
        /// 更新头像
        /// </summary>
        /// <param name="id"></param>
        /// <param name="portrait"></param>
        /// <returns></returns>
        Task<ChatObjectDto> UpdatePortraitAsync(long id, string portrait);

        Task<ChatObjectDto> SetVerificationMethodAsync(long id, VerificationMethods verificationMethod);

        Task<ChatObjectDetailDto> GetDetailAsync(long id);

    }
}
