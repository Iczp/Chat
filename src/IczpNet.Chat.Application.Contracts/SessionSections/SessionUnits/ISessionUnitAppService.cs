using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionUnits;

public interface ISessionUnitAppService
{
    Task<PagedResultDto<SessionUnitDto>> GetListAsync(SessionUnitGetListInput input);

    Task<PagedResultDto<SessionUnitDto>> GetListByLinqAsync(SessionUnitGetListInput input);

    Task<SessionUnitDto> GetAsync(Guid id);

    Task<SessionUnitDetailDto> GetDetailAsync(Guid id);

    Task<SessionUnitDto> SetToppingAsync(Guid id, bool isTopping);

    Task<SessionUnitDto> SetReadedAsync(Guid id, Guid messageId, bool isForce);

    Task<SessionUnitDto> RemoveSessionAsync(Guid id);

    Task<SessionUnitDto> KillSessionAsync(Guid id);

    Task<SessionUnitDto> ClearMessageAsync(Guid id);

    Task<SessionUnitDto> DeleteMessageAsync(Guid id, Guid messageId);

    Task<PagedResultDto<MessageDto>> GetMessageListAsync(Guid id, SessionUnitGetMessageListInput input);

    Task<MessageDto> GetMessageAsync(Guid id, Guid messageId);

    Task<int> GetBadgeAsync(Guid ownerId);

    Task<PagedResultDto<SessionUnitOwnerDto>> GetSessionMemberListAsync(Guid id, SessionUnitGetSessionMemberListInput input);
}
