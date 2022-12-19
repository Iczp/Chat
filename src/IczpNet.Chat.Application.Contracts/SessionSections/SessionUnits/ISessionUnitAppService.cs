using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public interface ISessionUnitAppService
    {
        Task<SessionUnitDto> SetReadedAsync(Guid id, Guid messageId);

        Task<SessionUnitDto> RemoveAsync(Guid id);

        Task<SessionUnitDto> KillAsync(Guid id);

        Task<SessionUnitDto> ClearAsync(Guid id);

        Task<SessionUnitDto> DeleteMessageAsync(Guid id, Guid messageId);
    }
}
