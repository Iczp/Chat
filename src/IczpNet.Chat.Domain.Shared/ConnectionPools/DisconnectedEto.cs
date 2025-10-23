namespace IczpNet.Chat.ConnectionPools;

public class DisconnectedEto : ConnectedEto
{
    public DisconnectedEto() { }
    public DisconnectedEto(string connectionId)
    {
        ConnectionId = connectionId;
    }
}
