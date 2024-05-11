using Volo.Abp.Data;

namespace IczpNet.Chat.IpAddresses;

public class LocationInfo: IHasExtraProperties
{
    /// <summary>
    /// 
    /// </summary>
    public virtual string Ip { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public virtual string Address { get; set; }

    /// <summary>
    /// 国家
    /// </summary>
    public virtual string Country { get; set; }

    /// <summary>
    /// 省份
    /// </summary>
    public virtual string Region { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    public virtual string City { get; set; }

    /// <summary>
    /// 邮政编码
    /// </summary>
    public virtual string Zip { get; set; }

    /// <summary>
    /// 纬度
    /// </summary>
    public virtual float? Latitude { get; set; }

    /// <summary>
    /// 经度
    /// </summary>
    public virtual float? Longitude { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public ExtraPropertyDictionary ExtraProperties { get; set; }

}
