using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.SessionPermissions
{
    public class SessionPermission : DomainService, ISessionPermission
    {
        public async Task CheckAsync(string sessionPermissionDefinitionId, Guid sessionUnitId)
        {
            await Task.CompletedTask;
        }

        public Task<bool> IsGrantedAsync(string sessionPermissionDefinitionId, Guid sessionUnitId)
        {
            throw new NotImplementedException();
        }
    }
}
