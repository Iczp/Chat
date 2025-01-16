using System.ComponentModel;

namespace IczpNet.Chat.Enums;


public enum HttpMethods
{
    [Description("GET")]
    Get = 0,
    /// <summary>
    /// HttpPost
    /// </summary>
    [Description("POST")]
    Post = 1,
    /// <summary>
    /// HttpPut
    /// </summary>
    [Description("PUT")]
    Put = 2,
    /// <summary>
    /// HttpDelete
    /// </summary>
    [Description("Delete")]
    Delete = 3,
    /// <summary>
    /// 
    /// </summary>
    [Description("Patch")]
    Patch,
    /// <summary>
    /// 
    /// </summary>
    [Description("Options")]
    Options,
    /// <summary>
    /// 
    /// </summary>
    [Description("Head")]
    Head,
    /// <summary>
    /// 
    /// </summary>
    [Description("Trace")]
    Trace,
    /// <summary>
    /// 
    /// </summary>
    [Description("Connect")]
    Connect
}
