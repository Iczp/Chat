using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.OpenedRecorders;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections
{
    public interface ISessionManager
    {
        Task<bool> IsFriendshipAsync(Guid ownerId, Guid destinationId);
        Task<Friendship> CreateFriendshipAsync(Guid ownerId, Guid destinationId, bool isPassive, Guid? friendshipRequestId);
        Task<Friendship> CreateFriendshipAsync(ChatObject owner, ChatObject destination, bool isPassive, Guid? friendshipRequestId);
        Task<DateTime> DeleteFriendshipAsync(Guid ownerId, Guid destinationId);
        Task<DateTime?> HandlRequestAsync(Guid friendshipRequestId, bool isAgreed, string handlMessage);
        Task<OpenedRecorder> SetOpenedAsync(Guid ownerId, Guid destinationId, Guid messageId, string deviceId);
    }
}
