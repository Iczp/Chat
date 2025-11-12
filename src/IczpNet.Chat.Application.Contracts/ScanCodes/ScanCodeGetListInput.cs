using IczpNet.AbpCommons.Dtos;

namespace IczpNet.Chat.ScanCodes;

public class ScanCodeGetListInput : GetListInput
{
    /// <summary>
    ///  处理器名称
    /// </summary>
    public virtual string HandlerFullName { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public virtual bool? Success { get; set; }

    /// <summary>
    /// Action
    /// </summary>

    public virtual string Action { get; set; }
}
