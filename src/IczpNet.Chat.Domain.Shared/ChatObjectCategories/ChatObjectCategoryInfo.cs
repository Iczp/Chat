using IczpNet.Chat.BaseInfos;
using System;

namespace IczpNet.Chat.ChatObjectCategories;

public class ChatObjectCategoryInfo : BaseTreeInfo<Guid>
{

    public virtual string ChatObjectTypeId { get; set; }
}
