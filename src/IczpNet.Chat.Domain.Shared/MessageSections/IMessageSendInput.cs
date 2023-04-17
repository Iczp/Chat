using System;

namespace IczpNet.Chat.MessageSections
{
    public interface IMessageSendInput<T> : IMessageSendInput
    {
        T Content { get; }
    }

    public interface IMessageSendInput
    {
        Guid SessionUnitId { get; set; }

        string KeyName { get; set; }

        string KeyValue { get; set; }

        long? QuoteMessageId { get; set; }
    }
}