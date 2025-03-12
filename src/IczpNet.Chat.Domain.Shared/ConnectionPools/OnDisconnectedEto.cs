namespace IczpNet.Chat.ConnectionPools;

public class OnDisconnectedEto
{
    public OnDisconnectedEto() { }
    public OnDisconnectedEto(string connectionId)
    {
        ConnectionId = connectionId;
    }

    public string ConnectionId { get; set; }
}
