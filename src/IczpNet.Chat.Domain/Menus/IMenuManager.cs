using IczpNet.AbpTrees;
using IczpNet.Chat.HttpRequests;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Menus;

public interface IMenuManager : ITreeManager<Menu, Guid, MenuInfo>
{
    Task<string> TriggerAsync(Guid id, Guid? sessionUnitId = null, long? senderId = null);

    Task<HttpRequest> SendToRemoteHostAsync(Menu menu, MenuTriggerArgs args = null, string name = null);

    Task CheckMenuAsync(Menu menu);

    Task<bool> IsCheckMenuAsync(Menu menu);
}
