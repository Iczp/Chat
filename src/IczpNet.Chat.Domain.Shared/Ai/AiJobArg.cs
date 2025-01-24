using IczpNet.Chat.Enums;

namespace IczpNet.Chat.Ai;

public class AiJobArg
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public EventTypes EventType { get; set; }

    /// <summary>
    /// 消息Id
    /// </summary>
    public long MessageId { get; set; }

    /// <summary>
    /// Ai提供者
    /// </summary>
    public string Provider { get; set; }

    public override string ToString()
    {
        return $"EventType={EventType},Provider={Provider},MessageId={MessageId}";
    }
}
