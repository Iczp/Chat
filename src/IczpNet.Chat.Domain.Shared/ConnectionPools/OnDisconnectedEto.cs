namespace IczpNet.Chat.ConnectionPools;

public class OnDisconnectedEto : OnConnectedEto
{
    public OnDisconnectedEto() { }
    public OnDisconnectedEto(string connectionId)
    {
        ConnectionId = connectionId;
    }
}
