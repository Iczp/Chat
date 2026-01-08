using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 消息报表类型
/// </summary>
[Description("消息报表类型")]
public enum MessageReportTypes
{
    ///// <summary>  
    ///// 年 
    ///// </summary>  
    //[Description("年")]
    //Year = 10,

    /// <summary>  
    /// 月    
    /// </summary>  
    [Description("月")]
    Month = 20,

    /// <summary>  
    /// 日
    /// </summary>  
    [Description("日")]
    Day = 30,

    /// <summary>  
    /// 时
    /// </summary>  
    [Description("时")]
    Hour = 40,
}