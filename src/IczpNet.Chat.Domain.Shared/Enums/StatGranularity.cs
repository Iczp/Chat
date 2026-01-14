using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IczpNet.Chat.Enums;

public enum StatGranularity
{
    ///// <summary>  
    ///// 年 
    ///// </summary>  
    //[Description("年")]
    //Year = 10,

    ///// <summary>  
    ///// 月    
    ///// </summary>  
    //[Description("月")]
    //Month = 20,

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

    /// <summary>  
    /// 分钟
    /// </summary>  
    [Description("分钟")]
    Minute = 50,
}
