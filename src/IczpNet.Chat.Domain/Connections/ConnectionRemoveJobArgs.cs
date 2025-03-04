namespace IczpNet.Chat.Connections;

public class ConnectionRemoveJobArgs(string connectionId)
{
    public string ConnectionId { get; set; } = connectionId;
}
