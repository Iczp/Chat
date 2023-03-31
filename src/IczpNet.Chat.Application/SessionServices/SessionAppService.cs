using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.OpenedRecorders;
using IczpNet.Chat.SessionSections.OpenedRecordes.Dtos;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.Sessions.Dtos;
using IczpNet.Chat.SessionSections.SessionTagDtos.Dtos;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using IczpNet.Chat.Specifications;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionServices
{
    public class SessionAppService : ChatAppService, ISessionAppService
    {

        protected IRepository<Friendship, Guid> FriendshipRepository { get; }
        protected IRepository<Session, Guid> Repository { get; }
        protected ISessionManager SessionManager { get; }
        protected ISessionGenerator SessionGenerator { get; }

        public SessionAppService(
            IRepository<Friendship, Guid> chatObjectRepository,
            ISessionManager sessionManager,
            ISessionGenerator sessionGenerator,
            IRepository<Session, Guid> repository)
        {
            FriendshipRepository = chatObjectRepository;
            SessionManager = sessionManager;
            SessionGenerator = sessionGenerator;
            Repository = repository;
        }


        public async Task<PagedResultDto<ChatObjectDto>> GetFriendsAsync(long ownerId, bool? isCantacts, int maxResultCount = 10, int skipCount = 0, string sorting = null)
        {
            var query = (await FriendshipRepository.GetQueryableAsync())
                .Where(x => x.OwnerId == ownerId)
                //.Where(x => x.IsPassive)
                .WhereIf(isCantacts.HasValue, x => x.IsCantacts)
                .Select(x => x.Destination)
                .Distinct()
                ;

            return await GetPagedListAsync<ChatObject, ChatObjectDto>(query, maxResultCount, skipCount, sorting);
        }

        public Task<DateTime> RequestForFriendshipAsync(long ownerId, long friendId, string message)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<OpenedRecorderDto> SetOpenedAsync(OpenedRecorderInput input)
        {
            var entity = await SessionManager.SetOpenedAsync(input.OwnerId, input.DestinationId, input.MessageId, input.DeviceId);

            return ObjectMapper.Map<OpenedRecorder, OpenedRecorderDto>(entity);
        }

        [HttpGet]
        public async Task<PagedResultDto<SessionDto>> GetListAsync(SessionGetListInput input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(input.OwnerId.HasValue, x => x.UnitList.Any(m => m.OwnerId == input.OwnerId))
                ;
            return await GetPagedListAsync<Session, SessionDto>(query, input, q => q.OrderByDescending(x => x.LastMessageId));
        }

        [HttpGet]
        public async Task<SessionDto> GetAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            return await MapToDtoAsync(entity);
        }

        protected virtual Task<SessionDto> MapToDtoAsync(Session entity)
        {
            return Task.FromResult(ObjectMapper.Map<Session, SessionDto>(entity));
        }

        [HttpGet]
        public async Task<SessionDetailDto> GetDetailAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            return await MapToDetailDtoAsync(entity);
        }

        protected virtual Task<SessionDetailDto> MapToDetailDtoAsync(Session entity)
        {
            return Task.FromResult(ObjectMapper.Map<Session, SessionDetailDto>(entity));
        }

        [HttpGet]
        public async Task<PagedResultDto<MessageDto>> GetMessageListAsync(Guid id, SessionGetMessageListInput input)
        {
            var query = (await Repository.GetAsync(id))
                .MessageList.AsQueryable()
                .WhereIf(!input.SenderId.IsEmpty(), new SenderMessageSpecification(input.SenderId.GetValueOrDefault()).ToExpression())
                .WhereIf(!input.MinAutoId.IsEmpty(), new MinAutoIdMessageSpecification(input.MinAutoId.GetValueOrDefault()).ToExpression())
                .WhereIf(!input.MaxAutoId.IsEmpty(), new MaxAutoIdMessageSpecification(input.MaxAutoId.GetValueOrDefault()).ToExpression())
                ;

            return await GetPagedListAsync<Message, MessageDto>(query, input, x => x.OrderByDescending(x => x.Id));
        }

        [HttpGet]
        public async Task<PagedResultDto<SessionTagDto>> GetTagListAsync(SessionTagGetListInput input)
        {
            var query = (await Repository.GetAsync(input.SessionId))
                .TagList.AsQueryable()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword))
                ;

            return await GetPagedListAsync<SessionTag, SessionTagDto>(query, input);
        }

        [HttpGet]
        public async Task<PagedResultDto<SessionRoleDto>> GetRoleListAsync(SessionRoleGetListInput input)
        {
            var query = (await Repository.GetAsync(input.SessionId))
                .RoleList.AsQueryable()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword))
                ;

            return await GetPagedListAsync<SessionRole, SessionRoleDto>(query, input);
        }

        [HttpGet]
        [Obsolete("Move to SessionUnitAppService.GetListBySessionIdAsync")]
        public async Task<PagedResultDto<SessionUnitOwnerDto>> GetSessionUnitListAsync(SessionGetListBySessionIdInput input)
        {
            var query = (await Repository.GetAsync(input.SessionId))
                .UnitList.AsQueryable()
                .Where(x => !x.IsKilled)
                .WhereIf(input.OwnerIdList.IsAny(), x => input.OwnerIdList.Contains(x.OwnerId))
                .WhereIf(input.OwnerTypeList.IsAny(), x => input.OwnerTypeList.Contains(x.Owner.ObjectType.Value))
                .WhereIf(!input.TagId.IsEmpty(), x => x.SessionUnitTagList.Any(x => x.SessionTagId == input.TagId))
                .WhereIf(!input.RoleId.IsEmpty(), x => x.SessionUnitRoleList.Any(x => x.SessionRoleId == input.RoleId))
                .WhereIf(!input.JoinWay.IsEmpty(), x => x.JoinWay == input.JoinWay)
                .WhereIf(!input.InviterId.IsEmpty(), x => x.InviterId == input.InviterId)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Owner.Name.Contains(input.Keyword))
                ;

            return await GetPagedListAsync<SessionUnit, SessionUnitOwnerDto>(query, input, q => q.OrderByDescending(x => x.Sorting).ThenByDescending(x => x.LastMessageId));
        }

        [HttpPost]
        public async Task<List<SessionDto>> GenerateSessionByMessageAsync()
        {
            var entitys = await SessionGenerator.GenerateSessionByMessageAsync();
            return ObjectMapper.Map<List<Session>, List<SessionDto>>(entitys);
        }

        [HttpPost]
        public async Task<SessionTagDto> AddTagAsync(Guid sessionId, string name)
        {
            var entity = await Repository.GetAsync(sessionId);

            var tag = await SessionManager.AddTagAsync(entity, new SessionTag(GuidGenerator.Create(), name));

            return ObjectMapper.Map<SessionTag, SessionTagDto>(tag);
        }

        [HttpPost]
        public Task RemoveTagAsync(Guid tagId)
        {
            return SessionManager.RemoveTagAsync(tagId);
        }

        [HttpPost]
        public async Task AddTagMembersAsync(Guid tagId, List<Guid> sessionUnitIdList)
        {
            await SessionManager.AddTagMembersAsync(tagId, sessionUnitIdList);
        }

        [HttpPost]
        public async Task RemoveTagMembersAsync(Guid tagId, List<Guid> sessionUnitIdList)
        {
            await SessionManager.RemoveTagMembersAsync(tagId, sessionUnitIdList);
        }

        [HttpPost]
        public async Task<SessionRoleDto> AddRoleAsync(Guid sessionId, string name)
        {
            var entity = await Repository.GetAsync(sessionId);

            var role = await SessionManager.AddRoleAsync(entity, new SessionRole(GuidGenerator.Create(), name));

            return ObjectMapper.Map<SessionRole, SessionRoleDto>(role);
        }

        [HttpPost]
        public Task RemoveRoleAsync(Guid roleId)
        {
            return SessionManager.RemoveRoleAsync(roleId);
        }

        [HttpPost]
        public async Task AddRoleMembersAsync(Guid roleId, List<Guid> sessionUnitIdList)
        {
            await SessionManager.AddRoleMembersAsync(roleId, sessionUnitIdList);
        }

        [HttpPost]
        public Task RemoveRoleMembersAsync(Guid roleId, List<Guid> sessionUnitIdList)
        {
            return SessionManager.RemoveRoleMembersAsync(roleId, sessionUnitIdList);
        }

        [HttpPost]
        public Task<List<SessionRoleDto>> SetRolesAsync(Guid sessionUnitId, List<Guid> roleIdList)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public Task<List<SessionTagDto>> SetTagsAsync(Guid sessionUnitId, List<Guid> tagIdList)
        {
            throw new NotImplementedException();
        }
    }
}
