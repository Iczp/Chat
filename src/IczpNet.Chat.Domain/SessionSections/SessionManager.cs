using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections
{
    public class SessionManager : DomainService, ISessionManager
    {
        public Task<DateTime> AddFriendAsync(Guid ownerId, Guid friendId)
        {
            throw new NotImplementedException();
        }

        public Task<DateTime> DeleteFriendAsync(Guid ownerId, Guid friendId)
        {
            throw new NotImplementedException();
        }
    }
}
