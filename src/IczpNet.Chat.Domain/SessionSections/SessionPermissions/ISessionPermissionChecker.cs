using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.SessionPermissions
{
    public interface ISessionPermissionChecker
    {
        //Task<bool> IsAuthenticatedAsync(string sessionPermissionDefinitionId, Guid sessionUnitId);
        Task<bool> IsGrantedAsync(string sessionPermissionDefinitionId, SessionUnit sessionUnit);

        Task CheckAsync(string sessionPermissionDefinitionId, SessionUnit sessionUnit);
    }
}
