using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.OfficialSections.Officials.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.OfficialSections.Officials
{
    public interface IOfficialAppService : IApplicationService
    {
        Task<ChatObjectDto> CreateAsync(OfficialCreateInput input);

        Task<SessionUnitOwnerDto> EnableAsync(long ownerId, long destinationId);

        Task<SessionUnitOwnerDto> EnableByIdAsync(Guid sessionUnitId);

        Task<SessionUnitOwnerDto> DisableAsync(Guid sessionUnitId);
    }
}
