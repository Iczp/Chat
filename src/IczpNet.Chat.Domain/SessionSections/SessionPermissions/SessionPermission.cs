using IczpNet.AbpCommons;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.SessionPermissions
{
    public class SessionPermission : DomainService, ISessionPermission
    {
        protected ISessionUnitManager SessionUnitManager { get; }
        protected ISessionPermissionDefinitionRepository Repository { get; }
        protected IRepository<SessionPermissionRoleGrant> SessionPermissionRoleGrantRepository { get; }
        public SessionPermission(ISessionUnitManager sessionUnitManager,
            ISessionPermissionDefinitionRepository sessionPermissionDefinitionRepository)
        {
            SessionUnitManager = sessionUnitManager;
            Repository = sessionPermissionDefinitionRepository;
        }

        public async Task CheckAsync(string sessionPermissionDefinitionId, Guid sessionUnitId)
        {
            await Task.CompletedTask;
        }

        public async Task<bool> IsGrantedAsync(string sessionPermissionDefinitionId, Guid sessionUnitId)
        {
            Assert.If(!SessionPermissionDefinitionConsts.GetAll().Contains(sessionPermissionDefinitionId), $"Key does not exist:{sessionPermissionDefinitionId}");

            var definition = await Repository.GetAsync(sessionPermissionDefinitionId);

            var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

            if (sessionUnit.UnitGrantList.Any(d => d.IsEnabled && d.DefinitionId == sessionPermissionDefinitionId))
            {
                return true;
            }

            if (sessionUnit.SessionUnitRoleList.Any(x => x.SessionRole.RoleGrantList.Any(d => d.IsEnabled && d.DefinitionId == sessionPermissionDefinitionId)))
            {
                return true;
            }
            
            return false;
        }
    }
}
