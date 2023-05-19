using IczpNet.Chat.Follows.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Follows
{
    public interface IFollowAppService
    {
        Task<PagedResultDto<SessionUnitDestinationDto>> GetListFollowingAsync(FollowingGetListInput input);

        Task<PagedResultDto<SessionUnitDestinationDto>> GetListFollowerAsync(FollowerGetListInput input);

        Task<bool> CreateAsync(FollowCreateInput input);

        Task DeleteAsync(Guid ownerId, List<Guid> idList);
    }
}
