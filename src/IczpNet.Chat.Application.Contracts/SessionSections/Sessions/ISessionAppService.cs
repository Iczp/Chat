using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.SessionSections.OpenedRecordes.Dtos;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.Sessions.Dtos;
using IczpNet.Chat.SessionSections.SessionTagDtos.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public interface ISessionAppService
    {
        Task<PagedResultDto<ChatObjectDto>> GetFriendsAsync(Guid ownerId, bool? isCantacts, int maxResultCount = 10, int skipCount = 0, string sorting = null);

        Task<OpenedRecorderDto> SetOpenedAsync(OpenedRecorderInput input);

        Task<PagedResultDto<SessionDto>> GetListAsync(SessionGetListInput input);

        Task<SessionDto> GetAsync(Guid id);

        Task<SessionDetailDto> GetDetailAsync(Guid id);

        Task<PagedResultDto<MessageDto>> GetMessageListAsync(Guid id, SessionGetMessageListInput input);

        Task<PagedResultDto<SessionTagDto>> GetTagListAsync(SessionTagGetListInput input);

        Task<PagedResultDto<SessionRoleDto>> GetRoleListAsync(SessionRoleGetListInput input);

        Task<PagedResultDto<SessionUnitOwnerDto>> GetSessionUnitListAsync(SessionSetUnitGetListInput input);

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
