using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.FriendshipRequests;
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
        protected IRepository<FriendshipRequest, Guid> FriendshipRequestRepository { get; }
        protected IChatObjectManager ChatObjectManager { get; }

        public SessionManager(
            IRepository<Friendship, Guid> friendshipRepository,
            IChatObjectManager chatObjectManager,
            IRepository<FriendshipRequest, Guid> friendshipRequestRepository)
        {
            FriendshipRepository = friendshipRepository;
            ChatObjectManager = chatObjectManager;
            FriendshipRequestRepository = friendshipRequestRepository;
        }

        public Task<bool> IsFriendshipAsync(Guid ownerId, Guid destinationId)
        {
            return FriendshipRepository.AnyAsync(x => x.OwnerId == ownerId && x.DestinationId == destinationId);
        }

        public async Task<Friendship> CreateFriendshipAsync(Guid ownerId, Guid destinationId, bool IsPassive, Guid? friendshipRequestId)
        {
            var owner = await ChatObjectManager.GetAsync(ownerId);

            var destination = await ChatObjectManager.GetAsync(destinationId);

            return await CreateFriendshipAsync(owner, destination, IsPassive, friendshipRequestId);
        }

        public async Task<Friendship> CreateFriendshipAsync(ChatObject owner, ChatObject destination, bool IsPassive, Guid? friendshipRequestId)
        {
            Assert.NotNull(owner, nameof(owner));

            Assert.NotNull(destination, nameof(destination));

            var entity = await FriendshipRepository.FindAsync(x => x.OwnerId == owner.Id && x.DestinationId == destination.Id);

            entity ??= await FriendshipRepository.InsertAsync(new Friendship(owner, destination, IsPassive, friendshipRequestId), autoSave: true);

            return entity;
        }

        public Task<DateTime> DeleteFriendshipAsync(Guid ownerId, Guid destinationId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteFriendshipRequestAsync(Guid ownerId, Guid destinationId)
        {
            return FriendshipRequestRepository.DeleteAsync(x => 
                (x.OwnerId == ownerId && x.DestinationId == destinationId && !x.IsHandled) ||
                (x.OwnerId == destinationId && x.DestinationId == ownerId && !x.IsHandled)
            );
        }

        public async Task<DateTime?> HandlRequestAsync(Guid friendshipRequestId, bool isAgreed, string handlMessage)
        {
            var friendshipRequest = await FriendshipRequestRepository.GetAsync(friendshipRequestId);

            Assert.If(friendshipRequest.IsHandled, $"Already been handled:IsAgreed={friendshipRequest.IsAgreed}");

            if (isAgreed)
            {
                await CreateFriendshipAsync(friendshipRequest.Owner, friendshipRequest.Destination, IsPassive: true, friendshipRequest.Id);
                await CreateFriendshipAsync(friendshipRequest.Destination, friendshipRequest.Owner, IsPassive: false, friendshipRequest.Id);
                friendshipRequest.AgreeRequest(handlMessage);
            }
            else
            {
                friendshipRequest.DisagreeRequest(handlMessage);
            }
            await FriendshipRequestRepository.UpdateAsync(friendshipRequest, autoSave: true);

            await DeleteFriendshipRequestAsync(friendshipRequest.OwnerId, friendshipRequest.DestinationId.Value);

            return friendshipRequest.HandlTime;
        }

    }
}
