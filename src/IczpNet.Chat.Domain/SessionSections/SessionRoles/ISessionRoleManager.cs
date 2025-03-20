using IczpNet.Chat.SessionSections.SessionPermissions;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.SessionRoles;

public interface ISessionRoleManager
{
    Task<SessionRole> SetAllPermissionsAsync(Guid id, PermissionGrantValue permissionGrantValue);
}
