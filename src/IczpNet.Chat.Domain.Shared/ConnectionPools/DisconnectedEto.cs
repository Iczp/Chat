namespace IczpNet.Chat.ConnectionPools;

public class DisconnectedEto : ConnectionPoolCacheItem
{
    public DisconnectedEto() { }
    public DisconnectedEto(string connectionId)
    {
        ConnectionId = connectionId;
    }
}
