using System.Collections.Generic;

namespace IczpNet.Chat.ChatObjects;

public class ChatObjectSearchCacheItem(List<long> chatObjectIdList)
{
    public List<long> ChatObjectIdList { get; set; } = chatObjectIdList;

}
