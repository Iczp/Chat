using IczpNet.AbpCommons;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.SessionPermissions
{
    public class SessionPermissionChecker : DomainService, ISessionPermissionChecker
    {
        protected ISessionUnitManager SessionUnitManager { get; }
        protected ISessionPermissionDefinitionRepository Repository { get; }
        protected IRepository<SessionPermissionRoleGrant> SessionPermissionRoleGrantRepository { get; }
        public SessionPermissionChecker(ISessionUnitManager sessionUnitManager,
            ISessionPermissionDefinitionRepository sessionPermissionDefinitionRepository)
        {
            SessionUnitManager = sessionUnitManager;
            Repository = sessionPermissionDefinitionRepository;
        }


        public async Task CheckAsync(string sessionPermissionDefinitionId, SessionUnit sessionUnit)
        {
            Assert.If(!await IsGrantedAsync(sessionPermissionDefinitionId, sessionUnit),
                message: $"No permission:{(await Repository.GetAsync(sessionPermissionDefinitionId)).Name}",
                code: $"Permission:{sessionPermissionDefinitionId}");
        }

        public async Task CheckAsync(string sessionPermissionDefinitionId, Guid sessionUnitId)
        {
            Assert.If(!await IsGrantedAsync(sessionPermissionDefinitionId, sessionUnitId),
                message: $"No permission:{(await Repository.GetAsync(sessionPermissionDefinitionId)).Name}",
                code: $"Permission:{sessionPermissionDefinitionId}");
        }

        public async Task<bool> IsGrantedAsync(string sessionPermissionDefinitionId, Guid sessionUnitId)
        {
            var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

            return await IsGrantedAsync(sessionPermissionDefinitionId, sessionUnit);
        }

        public async Task<bool> IsGrantedAsync(string sessionPermissionDefinitionId, SessionUnit sessionUnit)
        {
            await Task.CompletedTask;

            Assert.If(!SessionPermissionDefinitionConsts.GetAll().Contains(sessionPermissionDefinitionId), $"Key does not exist:{sessionPermissionDefinitionId}");

            //UnitGrant
            if (sessionUnit.GrantList.Any(d => d.IsEnabled && d.DefinitionId == sessionPermissionDefinitionId))
            {
                Logger.LogDebug($"Role Permission IsGranted:{sessionPermissionDefinitionId}");
                return true;
            }

            //RoleGrant
            if (sessionUnit.SessionUnitRoleList.Any(x => x.SessionRole.GrantList.Any(d => d.IsEnabled && d.DefinitionId == sessionPermissionDefinitionId)))
            {
                Logger.LogDebug($"SessionUnit Permission IsGranted:{sessionPermissionDefinitionId}");
                return true;
            }

            Logger.LogDebug($"Not granted permission:{sessionPermissionDefinitionId}");

            return false;
        }

       
    }
}
