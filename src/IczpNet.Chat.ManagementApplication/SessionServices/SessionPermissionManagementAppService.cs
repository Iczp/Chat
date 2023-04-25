using IczpNet.Chat.Management.BaseAppServices;
using IczpNet.Chat.Management.SessionSections.SessionPermissionDefinitions.Dtos;
using IczpNet.Chat.Management.SessionSections.SessionPermissions;
using IczpNet.Chat.Management.SessionSections.SessionPermissions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Management.SessionServices
{
    public class SessionPermissionManagementAppService : ChatManagementAppService, ISessionPermissionManagementAppService
    {
        protected ISessionUnitRepository SessionUnitRepository { get; }
        protected IRepository<SessionRole, Guid> SessionRoleRepository { get; }
        protected ISessionPermissionDefinitionRepository SessionPermissionDefinitionRepository { get; }
        public SessionPermissionManagementAppService(ISessionUnitRepository sessionUnitRepository,
            ISessionPermissionDefinitionRepository sessionPermissionDefinitionRepository,
            IRepository<SessionRole, Guid> sessionRoleRepository)
        {
            SessionUnitRepository = sessionUnitRepository;
            SessionPermissionDefinitionRepository = sessionPermissionDefinitionRepository;
            SessionRoleRepository = sessionRoleRepository;
        }

        [HttpGet]
        public Task<Dictionary<string, PermissionGrantValue>> GetGrantedBySessionRoleAsync(Guid sessionRoleId)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public async Task<SessionPermissionGrantDto> GetGrantedBySessionUnitAsync(string permissionDefinitionId, Guid sessionUnitId)
        {
            var permissionDefinition = await SessionPermissionDefinitionRepository.GetAsync(permissionDefinitionId);

            var sessionUnit = await SessionUnitRepository.GetAsync(sessionUnitId);

            var roleGrants = sessionUnit.SessionUnitRoleList.SelectMany(x => x.SessionRole.GrantList.Where(d => d.DefinitionId == permissionDefinitionId)).ToList();

            var unitGrants = sessionUnit.GrantList.Where(x => x.DefinitionId == permissionDefinitionId).ToList();

            return new SessionPermissionGrantDto()
            {
                Definition = ObjectMapper.Map<SessionPermissionDefinition, SessionPermissionDefinitionDto>(permissionDefinition),
                RoleGrants = ObjectMapper.Map<List<SessionPermissionRoleGrant>, List<SessionPermissionRoleGrantDto>>(roleGrants),
                UnitGrants = ObjectMapper.Map<List<SessionPermissionUnitGrant>, List<SessionPermissionUnitGrantDto>>(unitGrants),
            };
        }

        [HttpPost]
        public async Task<SessionPermissionUnitGrantDto> GrantBySessionUnitAsync(string definitionId, Guid sessionUnitId, PermissionGrantValue permissionGrantValue)
        {
            var permissionDefinition = await SessionPermissionDefinitionRepository.GetAsync(definitionId);

            var sessionUnit = await SessionUnitRepository.GetAsync(sessionUnitId);

            var sessionPermissionUnitGrant = sessionUnit.GrantList.FirstOrDefault(x => x.DefinitionId == definitionId);

            if (sessionPermissionUnitGrant == null)
            {
                sessionUnit.GrantList.Add(new SessionPermissionUnitGrant(
                    definitionId: definitionId,
                    sessionUnitId: sessionUnitId,
                    value: permissionGrantValue.Value,
                    isEnabled: permissionGrantValue.IsEnabled));
            }
            else
            {
                sessionPermissionUnitGrant.Value = permissionGrantValue.Value;
                sessionPermissionUnitGrant.IsEnabled = permissionGrantValue.IsEnabled;
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<SessionPermissionUnitGrant, SessionPermissionUnitGrantDto>(sessionPermissionUnitGrant);
        }

        [HttpPost]
        public async Task<SessionPermissionRoleGrantDto> GrantBySessionRoleAsync(string definitionId, Guid sessionRoleId, PermissionGrantValue permissionGrantValue)
        {
            var permissionDefinition = await SessionPermissionDefinitionRepository.GetAsync(definitionId);

            var sessionRole = await SessionRoleRepository.GetAsync(sessionRoleId);

            var sessionPermissionUnitGrant = sessionRole.GrantList.FirstOrDefault(x => x.DefinitionId == definitionId);

            if (sessionPermissionUnitGrant == null)
            {
                sessionRole.GrantList.Add(new SessionPermissionRoleGrant(
                    definitionId: definitionId,
                    roleId: sessionRoleId,
                    value: permissionGrantValue.Value,
                    isEnabled: permissionGrantValue.IsEnabled));
            }
            else
            {
                sessionPermissionUnitGrant.Value = permissionGrantValue.Value;
                sessionPermissionUnitGrant.IsEnabled = permissionGrantValue.IsEnabled;
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<SessionPermissionRoleGrant, SessionPermissionRoleGrantDto>(sessionPermissionUnitGrant);
        }
    }
}
