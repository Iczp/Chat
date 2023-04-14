using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;

namespace IczpNet.Chat.Authorizations
{
    public class SessionPermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionName { get; }

        public SessionPermissionRequirement([NotNull] string permissionName)
        {
            Check.NotNull(permissionName, nameof(permissionName));

            PermissionName = permissionName;
        }

        public override string ToString()
        {
            return $"{nameof(SessionPermissionRequirement)}: {PermissionName}";
        }
    }
}
