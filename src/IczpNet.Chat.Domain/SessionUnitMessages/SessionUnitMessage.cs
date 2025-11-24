using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;

namespace IczpNet.Chat.SessionUnitMessages;

public class SessionUnitMessage : BaseEntity<long>, ISoftDelete
{

    /// <summary>
    /// 
    /// </summary>
    public virtual long? OwnerId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual Guid? SessionId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual Guid SessionUnitId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(SessionUnitId))]
    public virtual SessionUnit SessionUnit { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual long MessageId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(MessageId))]
    public virtual Message Message { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual bool IsRead { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual bool IsOpened { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual bool IsFavorited { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual bool IsFollowing { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual bool IsRemindMe { get; set; }

    ///// <summary>
    ///// 
    ///// </summary>
    //public virtual bool IsDeleted { get; set; }

    public override object[] GetKeys()
    {
        return [SessionUnitId, MessageId,];
    }
}
