using IczpNet.Chat.BaseDtos;
using System.ComponentModel;

namespace IczpNet.Chat.ReadedRecorders.Dtos;

public class GetListByMessageIdInput : GetListInput
{
    /// <summary>
    /// 是否已读
    /// </summary>
    [DefaultValue(true)]
    public virtual bool IsReaded { get; set; }
}
