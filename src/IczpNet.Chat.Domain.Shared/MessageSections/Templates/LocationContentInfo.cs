namespace IczpNet.Chat.MessageSections.Templates;

/// <summary>
/// 位置信息
/// </summary>
public class LocationContentInfo : MessageContentInfoBase, IContentInfo
{
    /// <summary>
    /// AMap(高德地图)、baidu(百度地图)
    /// </summary>
    public virtual string Provider { get; set; }

    /// <summary>
    /// 位置名称
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// 简要说明
    /// </summary>
    public virtual string Address { get; set; }

    /// <summary>
    /// 地图图片
    /// </summary>
    public virtual string Image { get; set; }

    /// <summary>
    /// 坐标 Latitude
    /// </summary>
    public virtual float Latitude { get; set; }

    /// <summary>
    /// 坐标 Longitude
    /// </summary>
    public virtual float Longitude { get; set; }
}