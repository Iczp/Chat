using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat;

public interface ICrudByChatObjectChatAppService<
    TGetOutputDto,
    TGetListOutputDto,
    TKey,
    TGetListInput,
    TCreateInput,
    TUpdateInput>
    where TKey : struct
{
    Task<TGetOutputDto> GetAsync(long ownerId, TKey id);

    Task<PagedResultDto<TGetListOutputDto>> GetListAsync(long ownerId, TGetListInput input);

    Task<TGetOutputDto> CreateAsync(long ownerId, TCreateInput input);

    Task<TGetOutputDto> UpdateAsync(long ownerId, TKey id, TUpdateInput input);

    Task DeleteByAsync(long ownerId, TKey id);

    Task DeleteManyAsync(long ownerId, List<TKey> idList);
}
