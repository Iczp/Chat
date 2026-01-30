using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionPermissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Authorizations
{
    public class ChatObjectAuthorizationHandler : AuthorizationHandler<ChatObjectRequirement, ChatObject>, ISingletonDependency
    {
        protected ISessionPermissionChecker SessionPermissionChecker { get; }
        protected ILogger<ChatObjectAuthorizationHandler> Logger { get; }

        public ChatObjectAuthorizationHandler(ISessionPermissionChecker sessionPermissionChecker,
            ILogger<ChatObjectAuthorizationHandler> logger)
        {
            SessionPermissionChecker = sessionPermissionChecker;
            Logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ChatObjectRequirement requirement, ChatObject resource)
        {
            Logger.LogInformation($"requirement:{requirement}");

            if (!SessionPermissionDefinitionConsts.GetAll().Contains(requirement.PermissionName))
            {
                return;
            }

            //if (await SessionPermissionChecker.IsGrantedAsync(requirement.PermissionName, resource))
            //{
            //    context.Succeed(requirement);
            //}

            await Task.Yield();
        }
    }
}
