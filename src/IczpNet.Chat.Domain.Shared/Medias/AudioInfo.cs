using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Medias;

public class AudioInfo
{
    /// <summary>
    /// 时间长度，单位为 （毫秒）
    /// </summary>
    public virtual TimeSpan? Duration { get; set; }

    /// <summary>
    /// 其他信息
    /// </summary>
    public virtual Dictionary<string, string> Profile { get; set; } = [];
}
