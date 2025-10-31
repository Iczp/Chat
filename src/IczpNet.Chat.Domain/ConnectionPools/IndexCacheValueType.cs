using System.ComponentModel;

namespace IczpNet.Chat.ConnectionPools;

public enum IndexCacheValueType
{
    ///// <summary>
    ///// 未设置
    ///// </summary>
    //[Description("未设置")]
    //Unset = 0,

    /// <summary>
    /// 连接Id
    /// </summary>
    [Description("连接Id")]
    ConnectionId = 1,

    /// <summary>
    /// 设备类型
    /// </summary>
    [Description("设备类型")]
    DeviceType = 2,
}
