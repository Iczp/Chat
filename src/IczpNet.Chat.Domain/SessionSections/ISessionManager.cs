using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.Friendships;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections
{
    public interface ISessionManager
    {
        Task<bool> IsFriendshipAsync(Guid ownerId, Guid friendId);
        Task<Friendship> CreateFriendshipAsync(Guid ownerId, Guid friendId, bool isPassive, Guid? friendshipRequestId);
        Task<Friendship> CreateFriendshipAsync(ChatObject owner, ChatObject friend, bool isPassive, Guid? friendshipRequestId);
        Task<DateTime> DeleteFriendshipAsync(Guid ownerId, Guid friendId);
        Task<DateTime?> HandlRequestAsync(Guid friendshipRequestId, bool isAgreed, string handlMessage);
    }
}
