using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionBoxes;

public class BoxCacheList
{
    public long OwnerId { get; set; }
    public List<BoxInfo> List { get; set; } = [];

    public static BoxCacheList Empty(long ownerId)
    {
        return new BoxCacheList()
        {
            OwnerId = ownerId,
        };
    }
}
