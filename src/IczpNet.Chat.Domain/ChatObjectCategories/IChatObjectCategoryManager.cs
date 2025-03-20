using IczpNet.AbpTrees;
using System;

namespace IczpNet.Chat.ChatObjectCategories;

public interface IChatObjectCategoryManager : ITreeManager<ChatObjectCategory, Guid, ChatObjectCategoryInfo>
{
}
