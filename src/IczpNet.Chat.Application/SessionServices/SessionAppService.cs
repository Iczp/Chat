using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.Sessions.Dtos;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionTags.Dtos;
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
        protected IRepository<Session, Guid> Repository { get; }
        protected ISessionManager SessionManager { get; }
        protected ISessionGenerator SessionGenerator { get; }

        public SessionAppService(
            ISessionManager sessionManager,
            ISessionGenerator sessionGenerator,
            IRepository<Session, Guid> repository)
        {
            SessionManager = sessionManager;
            SessionGenerator = sessionGenerator;
            Repository = repository;
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
            var query = (await Repository.GetAsync(input.SessionId.Value))
                .RoleList.AsQueryable()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword))
                ;

            return await GetPagedListAsync<SessionRole, SessionRoleDto>(query, input);
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

            var tag = await SessionManager.AddTagAsync(entity, new SessionTag(GuidGenerator.Create(), sessionId, name));

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

            var role = await SessionManager.AddRoleAsync(entity, new SessionRole(GuidGenerator.Create(), sessionId, name));

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
