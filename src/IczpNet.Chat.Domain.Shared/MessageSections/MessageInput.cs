using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace IczpNet.Chat.MessageSections;

public class MessageInput<T> : MessageInput, IMessageInput<T>
{
    public virtual T Content { get; set; }
}

public class MessageInput : IMessageInput
{
    ///// <summary>
    ///// SessionUnitId
    ///// </summary>
    //public virtual Guid SessionUnitId { get; set; }

    ///// <summary>
    ///// 扩展（键名）根据业务自义，如:"courseId"、"course-userId"、"erp-userId"
    ///// </summary>
    //[DefaultValue(null)]
    //public virtual string KeyName { get; set; }

    ///// <summary>
    ///// 扩展（键值）根据业务自义,如："123456789"、"02b7d668-02ca-428f-b88c-b8adac2c5044"、"admin"
    ///// </summary>
    //[DefaultValue(null)]
    //public virtual string KeyValue { get; set; }

    /// <summary>
    /// 接收人会话单元ID（私有消息才有） SessionUnitId
    /// </summary>
    public virtual Guid? ReceiverSessionUnitId { get; set; }

    /// <summary>
    /// 是否私有消息
    /// </summary>
    public virtual bool IsPrivate { get; set; }

    /// <summary>
    /// 引用消息Id
    /// </summary>
    [DefaultValue(null)]
    public virtual long? QuoteMessageId { get; set; }

    /// <summary>
    /// Ignore Connections
    /// </summary>
    [DefaultValue(null)]
    public virtual List<string> IgnoreConnections { get; set; } = [];

    /// <summary>
    /// Remind SessionUnitId
    /// </summary>
    [DefaultValue(null)]
    public virtual List<Guid> RemindList { get; set; } = [];
}
