using IczpNet.Chat.SessionPermissions;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionRoles;

public interface ISessionRoleManager
{
    Task<SessionRole> SetAllPermissionsAsync(Guid id, PermissionGrantValue permissionGrantValue);
}
