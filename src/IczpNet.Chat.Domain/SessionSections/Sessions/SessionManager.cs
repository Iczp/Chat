using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
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
        protected IRepository<Session, Guid> Repository { get; }
        protected IRepository<SessionUnit, Guid> SessionUnitRepository { get; }
        protected IRepository<SessionRole, Guid> SessionRoleRepository { get; }
        protected IRepository<SessionTag, Guid> SessionTagRepository { get; }

        public SessionManager(
            IRepository<Session, Guid> repository,
            IRepository<SessionRole, Guid> sessionRoleRepository,
            IRepository<SessionTag, Guid> sessionTagRepository,
            IRepository<SessionUnit, Guid> sessionUnitRepository)
        {
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

        public virtual async Task<bool> IsEnabledAsync(Guid sessionId)
        {
            return (await GetAsync(sessionId)).IsEnabled;
        }

        public Task<Session> GetAsync(Guid sessionId)
        {
            return Repository.GetAsync(sessionId);
        }

        public virtual async Task<Session> GetByKeyAsync(string sessionKey)
        {
            return Assert.NotNull(await Repository.FindAsync(x => x.SessionKey == sessionKey), $"No such session by sessionKey:{sessionKey}");
        }

        public virtual async Task<Session> GetByOwnerIdAsync(long roomId)
        {
            return Assert.NotNull(await Repository.FindAsync(x => x.OwnerId == roomId), $"No such session by roomId:{roomId}");
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
