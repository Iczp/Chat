using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums.Dtos;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageSections.Messages;

public interface IMessageAppService
{
    Task<List<EnumDto>> GetDisabledForwardListAsync();

    Task<PagedResultDto<MessageOwnerDto>> GetListAsync(MessageGetListInput input);

    Task<long> GetTotalCountAsync(MessageGetListInput input);

    Task<ExtraPagedResultDto<MessageOwnerDto>> GetListFastAsync(MessageFastGetListInput input);

    Task<ExtraPagedResultDto<MessageFastDto>> GetLatestAsync(MessageGetLatestInput input);

    Task<ExtraPagedResultDto<MessageFastDto>> GetHistoryAsync(MessageGetHistoryInput input);

    Task<PagedResultDto<MessageByDateDto>> GetListByDateAsync(MessageGetListByDateInput input);

    Task<int> BuildCacheAsync(Guid sessionId, long? minMessageId, int max = 5000, int batchSize = 1000);

    Task<bool> RemoveCacheAsync(Guid sessionId);

    Task<MessageOwnerDto> GetItemAsync(MessageGetItemInput input);

    Task<MessageOwnerDto> GetFileAsync(MessageGetItemInput input);
}
