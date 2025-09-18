using IczpNet.AbpTrees;
using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Enums;
using IczpNet.Chat.ServiceStates;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.ChatObjects;

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
    Task<PagedResultDto<ChatObjectDto>> GetListByUserIdAsync(Guid userId, GetListInput input);

    Task<PagedResultDto<ChatObjectDto>> GetListByCurrentUserAsync(GetListInput input);

    Task<ChatObjectDto> GetByCodeAsync(string code);

    Task<ChatObjectDto> CreateShopKeeperAsync(string name, string code);

    Task<ChatObjectDto> CreateShopWaiterAsync(long shopKeeperId, string name, string code);

    Task<ChatObjectDto> UpdateNameAsync(long id, string name);

    Task<ChatObjectDto> UpdatePortraitAsync(long id, string thumbnail, string portrait);

    Task<ChatObjectDto> SetVerificationMethodAsync(long id, VerificationMethods verificationMethod);

    Task<ChatObjectDetailDto> GetDetailAsync(long id);

    Task<ChatObjectProfileDto> GetProfileAsync(long id);

    Task<List<ServiceStatusCacheItem>> GetServiceStatusAsync(long id);

    Task<List<ServiceStatusCacheItem>> SetServiceStatusAsync(long id, ServiceStatus status);

    Task<ChatObjectDto> GenerateByUserAsync(Guid userId);

}
