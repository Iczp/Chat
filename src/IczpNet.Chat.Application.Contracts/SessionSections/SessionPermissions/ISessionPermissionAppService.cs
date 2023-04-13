using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.SessionPermissions
{
    public interface ISessionPermissionAppService
    {
        Task<Dictionary<string, PermissionGrantValue>> GetGrantByRoleAsync(Guid roleId);
    }
}
