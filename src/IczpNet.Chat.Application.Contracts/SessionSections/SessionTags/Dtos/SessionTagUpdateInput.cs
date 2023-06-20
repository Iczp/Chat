using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.SessionSections.SessionTags.Dtos;

public class SessionTagUpdateInput : BaseInput
{
    /// <summary>
    /// 标签名称
    /// </summary>
    public virtual string Name { get; set; }
}
