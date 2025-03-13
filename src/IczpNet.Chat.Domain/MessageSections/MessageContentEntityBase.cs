using Castle.DynamicProxy;
using IczpNet.AbpCommons.DataFilters;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.Attributes;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections;

/// <summary>
/// 消息内容实体基类
/// </summary>
public abstract class MessageContentEntityBase : BaseEntity<Guid>, IContentEntity, IIsActive, IIsEnabled, IChatOwner<long?>
{
    /// <summary>
    /// 是否已验证
    /// </summary>
    public virtual bool IsVerified { get; protected set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public virtual bool IsEnabled { get; protected set; } = true;

    /// <summary>
    /// 是否激活
    /// </summary>
    public virtual bool IsActive { get; protected set; }

    /// <summary>
    /// 所有者Id
    /// </summary>
    public virtual long? OwnerId { get; protected set; }

    /// <summary>
    /// 所有者
    /// </summary>
    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; protected set; }

    /// <summary>
    /// 消息列表
    /// </summary>
    public virtual IList<Message> MessageList { get; protected set; } = [];

    protected MessageContentEntityBase() { }

    protected MessageContentEntityBase(Guid id) : base(id) { }

    /// <summary>
    /// 获取消息体
    /// </summary>
    /// <returns></returns>
    public virtual string GetBody()
    {
        var currentContentType = ProxyUtil.GetUnproxiedType(this);
        var description = MessageTemplateAttribute.GetMessageType(currentContentType).GetDescription();
        return $"[{description}]";
    }

    /// <summary>
    /// 格式化字符串
    /// </summary>
    /// <param name="text"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public virtual string FormatString(string text, int length = 20)
    {
        return text.Length > 20 ? $"{text[..20]}…" : text;
    }

    /// <summary>
    /// 获取大小
    /// </summary>
    /// <returns></returns>
    public virtual long GetSize()
    {
        return 0;
    }

    /// <summary>
    /// 设置所有者Id
    /// </summary>
    /// <param name="ownerId"></param>
    public virtual void SetOwnerId(long? ownerId)
    {
        OwnerId = ownerId;
    }

    /// <summary>
    /// 设置Id
    /// </summary>
    /// <param name="guid"></param>
    public virtual void SetId(Guid guid)
    {
        Id = guid;
    }
}
