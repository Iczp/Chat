using IczpNet.Chat.Enums.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Enums;

public interface IEnumAppService
{
    Task<PagedResultDto<EnumTypeDto>> GetListAsync(EnumGetListInput input);

    Task<EnumTypeDto> GetAsync(string id);
}
