using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionTags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public interface ISessionManager
    {
        Task<Session> GetAsync(Guid sessionId);

        Task<Session> GetByOwnerIdAsync(long roomId);

        Task<Session> GetByKeyAsync(string sessionKey);

        Task<IQueryable<Session>> InSameAsync(long sourceChatObjectId, long destinationChatObjectId, ChatObjectTypeEnums? chatObjectType = null);

        Task<DateTime> DeleteFriendshipAsync(long ownerId, long destinationId);

        Task<SessionTag> AddTagAsync(Session entity, SessionTag sessionTag);

        Task RemoveTagAsync(Guid tagId);

        Task<SessionTag> AddTagMembersAsync(Guid tagId, List<Guid> sessionUnitIdList);

        Task<SessionTag> RemoveTagMembersAsync(Guid tagId, List<Guid> sessionUnitIdList);

        Task<SessionRole> AddRoleAsync(Session entity, SessionRole sessionRole);

        Task RemoveRoleAsync(Guid roleId);

        Task<SessionRole> AddRoleMembersAsync(Guid roleId, List<Guid> sessionUnitIdList);

        Task<SessionRole> RemoveRoleMembersAsync(Guid roleId, List<Guid> sessionUnitIdList);
        
    }
}
