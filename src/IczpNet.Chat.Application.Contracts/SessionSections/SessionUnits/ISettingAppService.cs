using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.SessionUnits;

public interface ISettingAppService
{
    Task<SessionUnitOwnerDto> SetMemberNameAsync(Guid sessionUnitId, string memberName);

    Task<SessionUnitOwnerDto> SetRenameAsync(Guid sessionUnitId, string rename);

    Task<SessionUnitOwnerDto> SetToppingAsync(Guid sessionUnitId, bool isTopping);

    Task<SessionUnitOwnerDto> SetReadedMessageIdAsync(Guid sessionUnitId, bool isForce = false, long? messageId = null);

    Task<SessionUnitOwnerDto> SetImmersedAsync(Guid sessionUnitId, bool isImmersed);

    Task<SessionUnitOwnerDto> RemoveAsync(Guid sessionUnitId);

    Task<SessionUnitOwnerDto> KillAsync(Guid sessionUnitId);

    Task<SessionUnitOwnerDto> ClearMessageAsync(Guid sessionUnitId);

    Task<SessionUnitOwnerDto> DeleteMessageAsync(Guid sessionUnitId, long messageId);
}
