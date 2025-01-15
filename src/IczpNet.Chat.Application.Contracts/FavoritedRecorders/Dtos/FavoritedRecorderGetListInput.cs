using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.FavoritedRecorders.Dtos;

public class FavoritedRecorderGetListInput : GetListInput
{
    /// <summary>
    /// 所有聊天对象
    /// </summary>
    public long? OwnerId { get; set; }

    /// <summary>
    /// 目标聊天对象
    /// </summary>
    public long? DestinationId { get; set; }

    /// <summary>
    /// 消息大小（最小值）
    /// </summary>
    public virtual long? MinSize { get; set; }

    /// <summary>
    /// 消息大小（最大值）
    /// </summary>
    public virtual long? MaxSize { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public virtual MessageTypes? MessageType { get; set; }
}
