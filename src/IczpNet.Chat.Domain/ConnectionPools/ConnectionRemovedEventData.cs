namespace IczpNet.Chat.ConnectionPools;

public class ConnectionRemovedEventData
{
    public ConnectionRemovedEventData()
    {
    }

    public ConnectionRemovedEventData(string connectionId)
    {
        ConnectionId = connectionId;
    }

    public string ConnectionId { get; set; }
}
