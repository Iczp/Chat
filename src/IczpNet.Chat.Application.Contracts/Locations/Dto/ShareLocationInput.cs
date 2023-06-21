using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Locations;

public class ShareLocationInput
{
    /// <summary>
    /// 会话单元列表
    /// </summary>
    public virtual List<Guid> SessionUnitIdList { get; set; }

    /// <summary>
    /// 用户位置
    /// </summary>
    public UserLocationCacheItem UserLocation { get; set; }
}
