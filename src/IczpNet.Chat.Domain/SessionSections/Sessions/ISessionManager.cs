using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.OpenedRecorders;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionTags;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public interface ISessionManager
    {
        Task<bool> IsFriendshipAsync(Guid ownerId, Guid destinationId);

        Task<Friendship> CreateFriendshipAsync(Guid ownerId, Guid destinationId, bool isPassive, Guid? friendshipRequestId);

        Task<Friendship> CreateFriendshipAsync(ChatObject owner, ChatObject destination, bool isPassive, Guid? friendshipRequestId);

        Task<DateTime> DeleteFriendshipAsync(Guid ownerId, Guid destinationId);

        Task<DateTime?> HandlRequestAsync(Guid friendshipRequestId, bool isAgreed, string handlMessage);

        Task<OpenedRecorder> SetOpenedAsync(Guid ownerId, Guid destinationId, Guid messageId, string deviceId);

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
