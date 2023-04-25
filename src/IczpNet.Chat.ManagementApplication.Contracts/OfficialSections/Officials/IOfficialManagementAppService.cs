using IczpNet.Chat.Management.ChatObjects.Dtos;
using IczpNet.Chat.Management.OfficialSections.Officials.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionUnits.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Management.OfficialSections.Officials
{
    public interface IOfficialManagementAppService : IApplicationService
    {
        Task<ChatObjectDto> CreateAsync(OfficialCreateInput input);

        //Task<ChatObjectDto> UpdateAsync(Guid id, OfficialUpdateInput input);

        Task<SessionUnitOwnerDto> SubscribeAsync(long ownerId, long destinationId);

        Task<SessionUnitOwnerDto> SubscribeByIdAsync(Guid sessionUnitId);

        Task<SessionUnitOwnerDto> UnsubscribeAsync(Guid sessionUnitId);
    }
}
