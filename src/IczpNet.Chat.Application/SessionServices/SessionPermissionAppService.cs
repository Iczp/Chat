using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionPermissions.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionServices
{
    public class SessionPermissionAppService : ChatAppService, ISessionPermissionAppService
    {
        protected ISessionUnitRepository SessionUnitRepository { get; }
        protected ISessionPermissionDefinitionRepository SessionPermissionDefinitionRepository { get; }
        public SessionPermissionAppService(ISessionUnitRepository sessionUnitRepository,
            ISessionPermissionDefinitionRepository sessionPermissionDefinitionRepository)
        {
            SessionUnitRepository = sessionUnitRepository;
            SessionPermissionDefinitionRepository = sessionPermissionDefinitionRepository;
        }

        public Task<Dictionary<string, PermissionGrantValue>> GetGrantedByRoleAsync(Guid roleId)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public async Task<SessionPermissionGrantDto> GetGrantedBySessionUnitAsync(string permissionDefinitionId, Guid sessionUnitId)
        {
            var permissionDefinition = await SessionPermissionDefinitionRepository.GetAsync(permissionDefinitionId);

            var sessionUnit = await SessionUnitRepository.GetAsync(sessionUnitId);

            var roleGrants = sessionUnit.SessionUnitRoleList.SelectMany(x => x.SessionRole.RoleGrantList.Where(d => d.DefinitionId == permissionDefinitionId)).ToList();

            var unitGrants = sessionUnit.UnitGrantList.ToList();

            return new SessionPermissionGrantDto()
            {
                Definition = ObjectMapper.Map<SessionPermissionDefinition, SessionPermissionDefinitionDto>(permissionDefinition),
                RoleGrants = ObjectMapper.Map<List<SessionPermissionRoleGrant>, List<SessionPermissionRoleGrantDto>>(roleGrants),
                UnitGrants = ObjectMapper.Map<List<SessionPermissionUnitGrant>, List<SessionPermissionUnitGrantDto>>(unitGrants),
            };
        }
    }
}
