using System.Collections.Generic;

namespace IczpNet.Chat.Locations;

/// <summary>
/// MediaLocation
/// </summary>
public class MediaLocation
{
    /// <summary>
    /// Media
    /// </summary>
    public virtual MediaItem Media { set; get; }

    /// <summary>
    /// UserLocationList
    /// </summary>
    public virtual List<UserLocationCacheItem> UserLocationList { set; get; }
}
