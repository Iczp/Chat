using System.Collections.Generic;

namespace IczpNet.Chat.ScanCodes;

public class ScanCodeResultDto
{
    /// <summary>
    /// 条码类型
    /// </summary>

    public virtual string Type { get; set; }

    /// <summary>
    /// 条码内容
    /// </summary>

    public virtual string Content { get; set; }

    /// <summary>
    /// 执行时间（毫秒）
    /// </summary>
    public virtual double Execution { get; set; }

    /// <summary>
    /// 执行时间（毫秒）
    /// </summary>
    public virtual int HandlerCount { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual IList<ScanHandlerResultDto> ScanHandlers { get; set; }
}
