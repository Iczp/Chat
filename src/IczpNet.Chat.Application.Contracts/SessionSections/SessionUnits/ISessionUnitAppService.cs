using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionUnits;

public interface ISessionUnitAppService
{
    Task<PagedResultDto<SessionUnitDto>> GetListAsync(SessionUnitGetListInput input);

    Task<PagedResultDto<SessionUnitDto>> GetListByLinqAsync(SessionUnitGetListInput input);

    Task<PagedResultDto<SessionUnitOwnerDto>> GetListBySessionIdAsync(SessionGetListBySessionIdInput input);

    Task<SessionUnitDto> GetAsync(Guid id);

    Task<SessionUnitDetailDto> GetDetailAsync(Guid id);

    Task<SessionUnitDto> SetToppingAsync(Guid id, bool isTopping);

    Task<SessionUnitDto> SetReadedAsync(Guid id, long messageId, bool isForce);

    Task<SessionUnitDto> SetImmersedAsync(Guid id, bool isImmersed);

    Task<SessionUnitDto> RemoveAsync(Guid id);

    Task<SessionUnitDto> KillAsync(Guid id);

    Task<SessionUnitDto> ClearMessageAsync(Guid id);

    Task<SessionUnitDto> DeleteMessageAsync(Guid id, long messageId);

    Task<PagedResultDto<MessageDto>> GetMessageListAsync(Guid id, SessionUnitGetMessageListInput input);

    Task<MessageDto> GetMessageAsync(Guid id, long messageId);

    Task<BadgeDto> GetBadgeAsync(long ownerId, bool? isImmersed = null);

    Task<List<BadgeDto>> GetBadgeByUserIdAsync(Guid userId, bool? isImmersed = null);

    Task<List<BadgeDto>> GetBadgeByCurrentUserAsync(bool? isImmersed = null);

    Task<PagedResultDto<SessionUnitOwnerDto>> GetSessionMemberListAsync(Guid id, SessionUnitGetSessionMemberListInput input);

    //Task<SessionUnitOwnerDto> GetSessionMemberAsync(Guid sessionId, Guid ownerId);
}
