namespace IczpNet.Chat.MessageSections;

public interface IMessageInput<T> : IMessageInput
{
    T Content { get; }
}

public interface IMessageInput
{
    //Guid SessionUnitId { get; set; }

    //string KeyName { get; set; }

    //string KeyValue { get; set; }

    long? QuoteMessageId { get; set; }

    /// <summary>
    /// 客户端消息Id
    /// </summary>
    string ClientMessageId { get; set; }
}