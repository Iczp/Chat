namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitSettingDto : SessionUnitSettingSimpleDto
{
    public virtual long? ReadedMessageId { get; set; }

    public virtual string Rename { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string Remarks { get; set; }

    /// <summary>
    /// 是否保存通讯录(群)
    /// </summary>
    public virtual bool IsContacts { get; set; }

    /// <summary>
    /// 消息免打扰，默认为 false
    /// </summary>
    public virtual bool IsImmersed { get; set; }

    /// <summary>
    /// 是否显示成员名称
    /// </summary>
    public virtual bool IsShowMemberName { get; set; }

    /// <summary>
    /// 是否显示已读
    /// </summary>
    public virtual bool IsShowReaded { get; set; }

    /// <summary>
    /// 聊天背景，默认为 null
    /// </summary>
    public virtual string BackgroundImage { get; set; }

}
