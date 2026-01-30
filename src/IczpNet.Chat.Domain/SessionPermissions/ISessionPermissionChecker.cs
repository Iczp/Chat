using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionUnits;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionPermissions;

public interface ISessionPermissionChecker
{
    //Task<bool> IsAuthenticatedAsync(string sessionPermissionDefinitionId, Guid sessionUnitId);
    Task<bool> IsGrantedAsync(string sessionPermissionDefinitionId, SessionUnit sessionUnit);

    Task CheckAsync(string sessionPermissionDefinitionId, SessionUnit sessionUnit);

    Task CheckAsync(string sessionPermissionDefinitionId, ChatObject chatObject);

    Task<bool> IsLoginAsync(ChatObject chatObject);

    Task CheckLoginAsync(ChatObject chatObject);


}
