using IczpNet.AbpTrees;
using System;

namespace IczpNet.Chat.ChatObjectCategorys
{
    public interface IChatObjectCategoryManager : ITreeManager<ChatObjectCategory, Guid, ChatObjectCategoryInfo>
    {
    }
}
