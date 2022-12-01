using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.Friendships;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections
{
    public class SessionManager : DomainService, ISessionManager
    {
        protected IRepository<Friendship, Guid> FriendshipRepository { get; }

        protected IChatObjectManager ChatObjectManager { get; }

        public SessionManager(
            IRepository<Friendship, Guid> friendshipRepository,
            IChatObjectManager chatObjectManager)
        {
            FriendshipRepository = friendshipRepository;
            ChatObjectManager = chatObjectManager;
        }

        public async Task<DateTime> AddFriendAsync(Guid ownerId, Guid friendId)
        {
            var owner = await ChatObjectManager.GetAsync(ownerId);

            var friend = await ChatObjectManager.GetAsync(friendId);

            var entty = await FriendshipRepository.FindAsync(x => x.OwnerId == ownerId && x.FriendId == friendId);

            entty ??= await FriendshipRepository.InsertAsync(new Friendship(owner, friend), autoSave: true);

            return entty.CreationTime;
        }

        public Task<DateTime> DeleteFriendAsync(Guid ownerId, Guid friendId)
        {
            throw new NotImplementedException();
        }
    }
}
