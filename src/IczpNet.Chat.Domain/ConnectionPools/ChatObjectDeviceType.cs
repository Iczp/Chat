namespace IczpNet.Chat.ConnectionPools;

public class ChatObjectDeviceType(long chatObjectId)
{
    public long ChatObjectId { get; set; } = chatObjectId;

    public override string ToString()
    {
        return $"{nameof(ChatObjectDeviceType)}:{ChatObjectId}";
    }
}