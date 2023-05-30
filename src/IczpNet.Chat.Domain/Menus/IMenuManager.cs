using IczpNet.AbpTrees;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Menus
{
    public interface IMenuManager : ITreeManager<Menu, Guid, MenuInfo>
    {
       Task<string> TriggerAsync(MenuTriggerArgs args);
    }
}
