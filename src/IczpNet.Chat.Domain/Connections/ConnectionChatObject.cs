using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Connections;

public class ConnectionChatObject : BaseEntity
{
    public virtual string ConnectionId { get; protected set; }

    [ForeignKey(nameof(ConnectionId))]
    public virtual Connection Connection { get; protected set; }

    public virtual long ChatObjectId { get; protected set; }

    [ForeignKey(nameof(ChatObjectId))]
    public virtual ChatObject ChatObject { get; protected set; }

    public override object[] GetKeys()
    {
        return [ConnectionId, ChatObjectId,];
    }

    protected ConnectionChatObject() { }

    public ConnectionChatObject(string connectionId, long chatObjectId)
    {
        ConnectionId = connectionId;
        ChatObjectId = chatObjectId;
    }
}
