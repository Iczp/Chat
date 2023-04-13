using IczpNet.AbpCommons;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.Sessions;
using Microsoft.Extensions.Logging;
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
            Assert.If(!await IsGrantedAsync(sessionPermissionDefinitionId, sessionUnitId),
                message: $"No permission:{(await Repository.GetAsync(sessionPermissionDefinitionId)).Name}",
                code: $"Permission:{sessionPermissionDefinitionId}");
        }

        public async Task<bool> IsGrantedAsync(string sessionPermissionDefinitionId, Guid sessionUnitId)
        {
            Assert.If(!SessionPermissionDefinitionConsts.GetAll().Contains(sessionPermissionDefinitionId), $"Key does not exist:{sessionPermissionDefinitionId}");

            //var definition = await Repository.GetAsync(sessionPermissionDefinitionId);

            var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

            //UnitGrant
            if (sessionUnit.UnitGrantList.Any(d => d.IsEnabled && d.DefinitionId == sessionPermissionDefinitionId))
            {
                Logger.LogDebug($"Role Permission IsGranted:{sessionPermissionDefinitionId}");
                return true;
            }

            //RoleGrant
            if (sessionUnit.SessionUnitRoleList.Any(x => x.SessionRole.RoleGrantList.Any(d => d.IsEnabled && d.DefinitionId == sessionPermissionDefinitionId)))
            {
                Logger.LogDebug($"SessionUnit Permission IsGranted:{sessionPermissionDefinitionId}");
                return true;
            }

            Logger.LogDebug($"Not granted permission:{sessionPermissionDefinitionId}");

            return false;
        }
    }
}
