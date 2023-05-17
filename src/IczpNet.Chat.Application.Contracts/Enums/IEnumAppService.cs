using IczpNet.Chat.Enums.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Enums
{
    public interface IEnumAppService
    {
        Task<PagedResultDto<EnumTypeDto>> GetAllAsync(EnumGetListInput input);

        Task<List<EnumDto>> GetListByTypeAsync(string type);
    }
}
