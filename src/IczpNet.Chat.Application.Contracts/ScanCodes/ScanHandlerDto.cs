using System;

namespace IczpNet.Chat.ScanCodes;

public class ScanHandlerDto
{
    /// <summary>
    /// 
    /// </summary>
    public virtual Guid ScanCodeId { get; set; }

    /// <summary>
    /// Action
    /// </summary>

    public virtual string Action { get; set; }

    /// <summary>
    /// 处理器
    /// </summary>

    public virtual string Handler { get; set; }

    /// <summary>
    /// 处理器
    /// </summary>

    public virtual string HandlerFullName { get; set; }

    /// <summary>
    /// 执行时间（毫秒）
    /// </summary>
    public virtual double Execution { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public virtual string Message { get; set; }

    /// <summary>
    /// 处理结果
    /// </summary>
    public virtual string Result { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public virtual bool Success { get; set; }
}
