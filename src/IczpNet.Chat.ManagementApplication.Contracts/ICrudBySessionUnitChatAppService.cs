using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat
{
    public interface ICrudBySessionUnitChatAppService<
        TGetOutputDto,
        TGetListOutputDto,
        TKey,
        TGetListInput,
        TCreateInput,
        TUpdateInput>
    {
        Task<TGetOutputDto> GetAsync(Guid sessionUnitId, TKey id);

        Task<PagedResultDto<TGetListOutputDto>> GetListAsync(Guid sessionUnitId, TGetListInput input);

        Task<TGetOutputDto> CreateAsync(Guid sessionUnitId, TCreateInput input);

        Task<TGetOutputDto> UpdateAsync(Guid sessionUnitId, TKey id, TUpdateInput input);

        Task DeleteByAsync(Guid sessionUnitId, TKey id);

        Task DeleteManyAsync(Guid sessionUnitId, List<TKey> idList);
    }
}
