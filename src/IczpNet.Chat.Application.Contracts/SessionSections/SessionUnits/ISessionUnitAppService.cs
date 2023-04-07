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
    Task<Guid?> FindIdAsync(long ownerId, long destinactionId);

    Task<PagedResultDto<SessionUnitOwnerDto>> GetListAsync(SessionUnitGetListInput input);

    Task<PagedResultDto<SessionUnitOwnerDto>> GetListByLinqAsync(SessionUnitGetListInput input);

    Task<PagedResultDto<SessionUnitDestinationDto>> GetListBySessionIdAsync(SessionGetListBySessionIdInput input);

    Task<SessionUnitOwnerDto> GetAsync(Guid id);

    Task<SessionUnitDetailDto> GetDetailAsync(Guid id);

    Task<SessionUnitOwnerDto> SetToppingAsync(Guid id, bool isTopping);

    Task<SessionUnitOwnerDto> SetReadedAsync(Guid id, long messageId, bool isForce);

    Task<SessionUnitOwnerDto> SetImmersedAsync(Guid id, bool isImmersed);

    Task<SessionUnitOwnerDto> RemoveAsync(Guid id);

    Task<SessionUnitOwnerDto> KillAsync(Guid id);

    Task<SessionUnitOwnerDto> ClearMessageAsync(Guid id);

    Task<SessionUnitOwnerDto> DeleteMessageAsync(Guid id, long messageId);

    Task<PagedResultDto<MessageDto>> GetMessageListAsync(Guid id, SessionUnitGetMessageListInput input);

    Task<MessageDto> GetMessageAsync(Guid id, long messageId);

    Task<BadgeDto> GetBadgeAsync(long ownerId, bool? isImmersed = null);

    Task<List<BadgeDto>> GetBadgeByUserIdAsync(Guid userId, bool? isImmersed = null);

    Task<List<BadgeDto>> GetBadgeByCurrentUserAsync(bool? isImmersed = null);

    Task<PagedResultDto<SessionUnitDestinationDto>> GetSessionMemberListAsync(Guid id, SessionUnitGetSessionMemberListInput input);

    //Task<SessionUnitOwnerDto> GetSessionMemberAsync(Guid sessionId, Guid ownerId);
}
