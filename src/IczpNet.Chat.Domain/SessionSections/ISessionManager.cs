using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections
{
    public interface ISessionManager
    {
        Task<DateTime> AddFriendAsync(Guid ownerId, Guid friendId);
        Task<DateTime> DeleteFriendAsync(Guid ownerId, Guid friendId);
    }
}
