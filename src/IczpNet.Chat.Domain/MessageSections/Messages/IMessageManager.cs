using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages;

public interface IMessageManager
{
    /// <summary>
    /// 创建消息
    /// </summary>
    /// <param name="senderSessionUnit"></param>
    /// <param name="action"></param>
    /// <param name="receiverSessionUnitId"></param>
    /// <param name="quoteMessageId"></param>
    /// <param name="remindList"></param>
    /// <returns></returns>
    Task<Message> CreateMessageAsync(SessionUnit senderSessionUnit,
        Func<Message, Task<IContentEntity>> action,
        Guid? receiverSessionUnitId = null,
        long? quoteMessageId = null,
        List<Guid> remindList = null);

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <typeparam name="TContentInfo"></typeparam>
    /// <typeparam name="TContentEntity"></typeparam>
    /// <param name="senderSessionUnit"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContentEntity>(
        SessionUnit senderSessionUnit,
        MessageInput<TContentInfo> input)
        where TContentInfo : IContentInfo
        where TContentEntity : IContentEntity;

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <typeparam name="TContentInfo"></typeparam>
    /// <typeparam name="TContentEntity"></typeparam>
    /// <param name="senderSessionUnit"></param>
    /// <param name="input"></param>
    /// <param name="contentEntity"></param>
    /// <returns></returns>
    Task<MessageInfo<TContentInfo>> SendAsync<TContentInfo, TContentEntity>(
        SessionUnit senderSessionUnit,
        MessageInput input,
        TContentEntity contentEntity)
        where TContentInfo : IContentInfo
        where TContentEntity : IContentEntity;

    /// <summary>
    /// 转发消息
    /// </summary>
    /// <param name="currentSessionUnitId"></param>
    /// <param name="sourceMessageId"></param>
    /// <param name="targetSessionUnitIdList"></param>
    /// <returns></returns>
    Task<List<Message>> ForwardAsync(Guid currentSessionUnitId, long sourceMessageId, List<Guid> targetSessionUnitIdList);

    /// <summary>
    /// 回滚消息
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<Dictionary<string, long>> RollbackAsync(Message message);

    /// <summary>
    /// 是否提醒 @我
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    Task<bool> IsRemindAsync(long messageId, Guid sessionUnitId);
}
