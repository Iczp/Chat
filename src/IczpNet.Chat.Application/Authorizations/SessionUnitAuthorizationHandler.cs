using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace IczpNet.Chat.Authorizations
{
    public class SessionUnitAuthorizationHandler : AuthorizationHandler<SessionUnitPermissionRequirement, SessionUnit>, ISingletonDependency
    {
        protected ISessionPermissionChecker SessionPermissionChecker { get; }
        protected ICurrentUser CurrentUser { get; }
        protected ILogger<SessionUnitAuthorizationHandler> Logger { get; }

        public SessionUnitAuthorizationHandler(
            ISessionPermissionChecker sessionPermissionChecker,
            ILogger<SessionUnitAuthorizationHandler> logger,
            ICurrentUser currentUser)
        {
            SessionPermissionChecker = sessionPermissionChecker;
            Logger = logger;
            CurrentUser = currentUser;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SessionUnitPermissionRequirement requirement, SessionUnit resource)
        {
            Logger.LogInformation($"requirement:{requirement}");

            if (!CurrentUser.IsIn(resource.OwnerId))
            {
                context.Fail();
            }

            if (!SessionPermissionDefinitionConsts.GetAll().Contains(requirement.PermissionName))
            {
                return;
            }

            if (await SessionPermissionChecker.IsGrantedAsync(requirement.PermissionName, resource))
            {
                context.Succeed(requirement);
            }

            await Task.Yield();
        }
    }
}
