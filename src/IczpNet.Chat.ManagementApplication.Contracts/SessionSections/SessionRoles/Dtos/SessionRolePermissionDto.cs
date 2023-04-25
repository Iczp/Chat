using IczpNet.Chat.SessionSections.SessionPermissions;
using System.Collections.Generic;

namespace IczpNet.Chat.Management.SessionSections.SessionRoles.Dtos
{
    public class SessionRolePermissionDto : SessionRoleDto
    {
        public Dictionary<string, PermissionGrantValue> PermissionGrant { get; set; }
    }
}
