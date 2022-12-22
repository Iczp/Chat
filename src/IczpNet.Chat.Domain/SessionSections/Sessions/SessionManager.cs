using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.FriendshipRequests;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.OpenedRecorders;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionUnitRoles;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnitTags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionManager : DomainService, ISessionManager
    {
        protected IRepository<Friendship, Guid> FriendshipRepository { get; }
        protected IRepository<FriendshipRequest, Guid> FriendshipRequestRepository { get; }
        protected IChatObjectManager ChatObjectManager { get; }
        protected IRepository<OpenedRecorder, Guid> OpenedRecorderRepository { get; }
        protected IRepository<Message, Guid> MessageRepository { get; }
        protected IRepository<Session, Guid> Repository { get; }
        protected IRepository<SessionUnit, Guid> SessionUnitRepository { get; }
        protected IRepository<SessionRole, Guid> SessionRoleRepository { get; }
        protected IRepository<SessionTag, Guid> SessionTagRepository { get; }
        protected ISessionRecorder SessionRecorder { get; }

        public SessionManager(
            IRepository<Friendship, Guid> friendshipRepository,
            IChatObjectManager chatObjectManager,
            IRepository<FriendshipRequest, Guid> friendshipRequestRepository,
            IRepository<OpenedRecorder, Guid> openedRecorderRepository,
            IRepository<Message, Guid> messageRepository,
            ISessionRecorder sessionRecorder,
            IRepository<Session, Guid> repository,
            IRepository<SessionRole, Guid> sessionRoleRepository,
            IRepository<SessionTag, Guid> sessionTagRepository,
            IRepository<SessionUnit, Guid> sessionUnitRepository)
        {
            FriendshipRepository = friendshipRepository;
            ChatObjectManager = chatObjectManager;
            FriendshipRequestRepository = friendshipRequestRepository;
            OpenedRecorderRepository = openedRecorderRepository;
            MessageRepository = messageRepository;
            SessionRecorder = sessionRecorder;
            Repository = repository;
            SessionRoleRepository = sessionRoleRepository;
            SessionTagRepository = sessionTagRepository;
            SessionUnitRepository = sessionUnitRepository;
        }

        protected async Task<Session> SetEntityAsync(Session entity, Action<Session> action = null)
        {
            action?.Invoke(entity);
            return await Repository.UpdateAsync(entity, autoSave: true);
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
                x.OwnerId == ownerId && x.DestinationId == destinationId && !x.IsHandled ||
                x.OwnerId == destinationId && x.DestinationId == ownerId && !x.IsHandled
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

        public async Task<OpenedRecorder> SetOpenedAsync(Guid ownerId, Guid destinationId, Guid messageId, string deviceId)
        {
            var message = await MessageRepository.GetAsync(messageId);

            var openedRecorder = await OpenedRecorderRepository.FindAsync(x => x.OwnerId == ownerId && x.DestinationId == destinationId);

            if (openedRecorder == null)
            {
                return await OpenedRecorderRepository.InsertAsync(new OpenedRecorder(ownerId, destinationId, message, deviceId), autoSave: true);
            }

            openedRecorder.SetMessage(message, deviceId);

            return await OpenedRecorderRepository.UpdateAsync(openedRecorder, autoSave: true);
        }

        public async Task<SessionTag> AddTagAsync(Session entity, SessionTag sessionTag)
        {
            await SetEntityAsync(entity, x => x.AddTag(sessionTag));

            return sessionTag;
        }

        public async Task RemoveTagAsync(Guid tagId)
        {
            var tag = await SessionTagRepository.GetAsync(tagId);

            var count = tag.SessionUnitTagList.Count();

            Assert.If(count > 0, $"Cannot delete tag[{tag}],there has {count} members");

            await SessionTagRepository.DeleteAsync(tag);
        }

        public async Task<SessionRole> AddRoleAsync(Session entity, SessionRole sessionRole)
        {
            await SetEntityAsync(entity, x => x.AddRole(sessionRole));

            return sessionRole;
        }

        public async Task RemoveRoleAsync(Guid roleId)
        {
            var role = await SessionRoleRepository.GetAsync(roleId);

            var count = role.SessionUnitRoleList.Count();

            Assert.If(count > 0, $"Cannot delete role[{role}],there has {count} members");

            await SessionRoleRepository.DeleteAsync(role);
        }

        public async Task<SessionTag> AddTagMembersAsync(Guid tagId, List<Guid> sessionUnitIdList)
        {
            var sessionTag = await SessionTagRepository.GetAsync(tagId);

            foreach (var sessionUnitId in sessionUnitIdList)
            {
                if (sessionTag.SessionUnitTagList.Any(x => x.SessionUnitId == sessionUnitId))
                {
                    continue;
                }
                var sessionUnit = await SessionUnitRepository.GetAsync(sessionUnitId);

                Assert.If(sessionTag.SessionId != sessionUnit.SessionId, "");

                sessionTag.SessionUnitTagList.Add(new SessionUnitTag(sessionTag, sessionUnit));
            }
            return await SessionTagRepository.UpdateAsync(sessionTag, true);
        }

        public async Task<SessionTag> RemoveTagMembersAsync(Guid tagId, List<Guid> sessionUnitIdList)
        {
            var sessionTag = await SessionTagRepository.GetAsync(tagId);

            var items = sessionTag.SessionUnitTagList.Where(x => sessionUnitIdList.Contains(x.SessionUnitId)).ToList();

            foreach (var item in items)
            {
                sessionTag.SessionUnitTagList.Remove(item);
            }
            return await SessionTagRepository.UpdateAsync(sessionTag, true);
        }

        public async Task<SessionRole> AddRoleMembersAsync(Guid roleId, List<Guid> sessionUnitIdList)
        {
            var sessionRole = await SessionRoleRepository.GetAsync(roleId);

            foreach (var sessionUnitId in sessionUnitIdList)
            {
                if (sessionRole.SessionUnitRoleList.Any(x => x.SessionUnitId == sessionUnitId))
                {
                    continue;
                }
                var sessionUnit = await SessionUnitRepository.GetAsync(sessionUnitId);

                Assert.If(sessionRole.SessionId != sessionUnit.SessionId, "");

                sessionRole.SessionUnitRoleList.Add(new SessionUnitRole(sessionRole, sessionUnit));
            }
            return await SessionRoleRepository.UpdateAsync(sessionRole, true);
        }

        public async Task<SessionRole> RemoveRoleMembersAsync(Guid roleId, List<Guid> sessionUnitIdList)
        {
            var sessionTag = await SessionRoleRepository.GetAsync(roleId);

            var items = sessionTag.SessionUnitRoleList.Where(x => sessionUnitIdList.Contains(x.SessionUnitId)).ToList();

            foreach (var item in items)
            {
                sessionTag.SessionUnitRoleList.Remove(item);
            }
            return await SessionRoleRepository.UpdateAsync(sessionTag, true);
        }
    }
}
