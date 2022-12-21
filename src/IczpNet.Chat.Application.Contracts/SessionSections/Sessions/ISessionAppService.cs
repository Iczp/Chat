using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.SessionSections.OpenedRecordes.Dtos;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.Sessions.Dtos;
using IczpNet.Chat.SessionSections.SessionTagDtos.Dtos;
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

        Task<PagedResultDto<MessageDto>> GetMessageListAsync(SessionMessageGetListInput input);

        Task<PagedResultDto<SessionTagDto>> GetTagListAsync(SessionTagGetListInput input);

        Task<PagedResultDto<SessionRoleDto>> GetRoleListAsync(SessionRoleGetListInput input);

        Task<SessionTagDto> AddTagAsync(Guid sessionId, string name);

        Task AddTagMemberAsync(Guid tagId, List<Guid> sessionUnitIdList);

        Task RemoveTagAsync(Guid tagId);

        Task<SessionRoleDto> AddRoleAsync(Guid sessionId, string name);

        Task AddRoleMemberAsync(Guid roleId, List<Guid> sessionUnitIdList);

        Task RemoveRoleAsync(Guid roleId);

        Task<List<SessionDto>> GenerateSessionByMessageAsync();
    }
}
