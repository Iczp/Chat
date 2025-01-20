using IczpNet.Chat.Enums;

namespace IczpNet.Chat.Developers;

public class DeveloperJobArg
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public EventTypes EventType {  get; set; }
    /// <summary>
    /// 消息Id
    /// </summary>
    public long MessageId { get; set; }

    public override string ToString()
    {
        return $"EventType={EventType},MessageId={MessageId}";
    }
}
