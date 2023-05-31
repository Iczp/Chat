using IczpNet.AbpTrees;
using IczpNet.Chat.HttpRequests;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Menus
{
    public interface IMenuManager : ITreeManager<Menu, Guid, MenuInfo>
    {
        Task<string> TriggerAsync(Guid id);

        Task<HttpRequest> SendToRemoteHostAsync(Guid id, string name = null);
    }
}
