using IczpNet.Chat.Management.ChatObjects.Dtos;
using IczpNet.Chat.Management.MessageSections.Messages.Dtos;
using IczpNet.Chat.Management.SessionSections.OpenedRecordes.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.Management.SessionSections.Sessions.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionTags.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Management.SessionSections.Sessions
{
    public interface ISessionManagementAppService
    {
        Task<PagedResultDto<ChatObjectDto>> GetFriendsAsync(long ownerId, bool? isCantacts, int maxResultCount = 10, int skipCount = 0, string sorting = null);

        Task<OpenedRecorderManagementDto> SetOpenedAsync(OpenedRecorderManagementInput input);

        Task<PagedResultDto<SessionDto>> GetListAsync(SessionGetListInput input);

        Task<SessionDto> GetAsync(Guid id);

        Task<SessionDetailDto> GetDetailAsync(Guid id);

        Task<PagedResultDto<MessageDto>> GetMessageListAsync(Guid id, SessionGetMessageListInput input);

        Task<PagedResultDto<SessionTagDto>> GetTagListAsync(SessionTagGetListInput input);

        Task<PagedResultDto<SessionRoleDto>> GetRoleListAsync(SessionRoleGetListInput input);

        Task<PagedResultDto<SessionUnitDestinationDto>> GetSessionUnitListAsync(Guid id, SessionUnitGetListDestinationInput input);

        Task<SessionTagDto> AddTagAsync(Guid sessionId, string name);

        Task RemoveTagAsync(Guid tagId);

        Task AddTagMembersAsync(Guid tagId, List<Guid> sessionUnitIdList);

        Task RemoveTagMembersAsync(Guid tagId, List<Guid> sessionUnitIdList);

        Task<SessionRoleDto> AddRoleAsync(Guid sessionId, string name);

        Task RemoveRoleAsync(Guid roleId);

        Task AddRoleMembersAsync(Guid roleId, List<Guid> sessionUnitIdList);

        Task RemoveRoleMembersAsync(Guid roleId, List<Guid> sessionUnitIdList);

        Task<List<SessionRoleDto>> SetRolesAsync(Guid sessionUnitId, List<Guid> roleIdList);

        Task<List<SessionTagDto>> SetTagsAsync(Guid sessionUnitId, List<Guid> tagIdList);

        Task<List<SessionDto>> GenerateSessionByMessageAsync();
    }
}
