using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.SessionPermissions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionServices
{
    public class SessionPermissionAppService : ChatAppService, ISessionPermissionAppService
    {
        public Task<Dictionary<string, PermissionGrantValue>> GetGrantByRoleAsync(Guid roleId)
        {
            throw new NotImplementedException();
        }
    }
}
