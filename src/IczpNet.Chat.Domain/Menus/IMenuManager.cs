using IczpNet.AbpTrees;
using System;

namespace IczpNet.Chat.Menus
{
    public interface IMenuManager : ITreeManager<Menu, Guid, MenuInfo>
    {
       
    }
}
