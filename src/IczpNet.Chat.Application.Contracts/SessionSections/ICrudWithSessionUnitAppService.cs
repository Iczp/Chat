using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections
{
    public interface ICrudWithSessionUnitAppService<TGetOutputDto, TGetListOutputDto, TKey, in TGetListInput, in TCreateInput, in TUpdateInput>
        where TKey : struct
    {

        Task<TGetOutputDto> GetByAsync(Guid sessionUnitId, TKey id);

        Task<PagedResultDto<TGetListOutputDto>> GetListByAsync(Guid sessionUnitId, TGetListInput input);

        Task<TGetOutputDto> CreateByAsync(Guid sessionUnitId, TCreateInput input);

        Task<TGetOutputDto> UpdateByAsync(Guid sessionUnitId, TKey id, TUpdateInput input);

        Task DeleteByAsync(Guid sessionUnitId, TKey id);

        Task DeleteManyByAsync(Guid sessionUnitId, List<TKey> idList);
    }
}
