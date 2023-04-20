using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections
{
    public interface ICrudWithSessionUnitAppService<TGetOutputDto, TGetListOutputDto, in TKey, in TGetListInput, in TCreateInput, in TUpdateInput>
    {

        Task<TGetOutputDto> GetByAsync(Guid sessionUnitId, long id);

        Task<PagedResultDto<TGetListOutputDto>> GetListByAsync(Guid sessionUnitId, TGetListInput input);

        Task<TGetOutputDto> CreateByAsync(Guid sessionUnitId, TCreateInput input);

        Task<TGetOutputDto> UpdateByAsync(Guid sessionUnitId, long id, TUpdateInput input);

        Task DeleteByAsync(Guid sessionUnitId, long id);

        Task DeleteManyByAsync(Guid sessionUnitId, List<long> idList);
    }
}
