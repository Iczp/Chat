using IczpNet.Chat.SessionPermissions;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionRoles.Dtos;

public class SessionRolePermissionDto : SessionRoleDto
{
    public Dictionary<string, PermissionGrantValue> PermissionGrant { get; set; }
}
