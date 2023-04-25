using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Authorizations
{
    public class SessionUnitAuthorizationHandler : AuthorizationHandler<SessionPermissionRequirement, SessionUnit>, ISingletonDependency
    {
        protected ISessionPermissionChecker SessionPermissionChecker { get; }
        protected ILogger<SessionUnitAuthorizationHandler> Logger { get; }

        public SessionUnitAuthorizationHandler(ISessionPermissionChecker sessionPermissionChecker,
            ILogger<SessionUnitAuthorizationHandler> logger)
        {
            SessionPermissionChecker = sessionPermissionChecker;
            Logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SessionPermissionRequirement requirement, SessionUnit resource)
        {
            Logger.LogInformation($"requirement:{requirement}");

            if (!SessionPermissionDefinitionConsts.GetAll().Contains(requirement.PermissionName))
            {
                return;
            }

            if (await SessionPermissionChecker.IsGrantedAsync(requirement.PermissionName, resource))
            {
                context.Succeed(requirement);
            }

            await Task.CompletedTask;
        }
    }
}
