﻿using Castle.DynamicProxy;
using IczpNet.AbpCommons;
using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using System.Collections;
using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections;

public static class MessageExtentions
{
    /// <summary>
    /// 禁止转发的消息类型
    /// </summary>
    public static List<MessageTypes> DisabledForwardList { get; set; } =
    [
        MessageTypes.Cmd,
        MessageTypes.RedEnvelope,
        MessageTypes.Html,
    ];

    /// <summary>
    /// 设置消息内容
    /// </summary>
    /// <param name="message"></param>
    /// <param name="messageContent"></param>
    public static void SetMessageContent(this Message message, IContentEntity messageContent)
    {
        Assert.NotNull(messageContent, $"MessageContent is null. message:{message}");

        var currentContentType = ProxyUtil.GetUnproxiedType(messageContent);

        var messageType = MessageTemplateAttribute.GetMessageType(currentContentType);

        Assert.NotNull(messageType, $"Item not exists. Key:'{messageType}'");

        message.SetMessageType(messageType);

        message.SetSize(messageContent.GetSize());

        var list = message.GetMessageContent();

        var genericType = list.GetType().GetGenericArguments()[0];

        Assert.If(genericType != currentContentType, $"'{currentContentType}' is not of type'{genericType}'");

        var currentInstance = ProxyUtil.GetUnproxiedInstance(messageContent);

        list.Add(currentInstance);

        ContentTypeAttribute.GetPropertyInfo(message.MessageType).SetValue(message, list, null);
    }

    /// <summary>
    /// 获取消息内容
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static IList GetMessageContent(this Message message)
    {
        var propertyInfo = ContentTypeAttribute.GetPropertyInfo(message.MessageType);

        var value = propertyInfo.GetValue(message, null);

        return (IList)value;
    }
}
