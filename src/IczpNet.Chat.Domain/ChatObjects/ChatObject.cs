using IczpNet.AbpCommons.DataFilters;
using IczpNet.AbpCommons.Extensions;
using IczpNet.AbpCommons.PinYin;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjectCategoryUnits;
using IczpNet.Chat.ChatObjectEntryValues;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Connections;
using IczpNet.Chat.Developers;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Mottos;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.SessionBoxs;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.SimpleStateChecking;

namespace IczpNet.Chat.ChatObjects;

[Index(nameof(Code))]
[Index(nameof(Name))]
[Index(nameof(FullPath))]
[Index(nameof(AppUserId))]
[Index(nameof(Gender))]
[Index(nameof(ObjectType))]
[Index(nameof(IsStatic))]
[Index(nameof(IsEnabled))]
[Index(nameof(IsPublic))]
[Index(nameof(IsEnabled))]
[Index(nameof(IsDefault))]
[Index(nameof(IsDeveloper))]

[Index(nameof(CreationTime), AllDescending = true)]

public class ChatObject : BaseTreeEntity<ChatObject, long>, IName, IChatObject, IHasSimpleStateCheckers<ChatObject>, IIsStatic, IIsActive, IIsPublic, IIsEnabled, IIsDefault, IAppUserId
{
    public List<ISimpleStateChecker<ChatObject>> StateCheckers { get; }

    //[StringLength(50)]
    //public virtual string TypeName { get; private set; }

    //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //public virtual long AutoId { get; }

    /// <summary>
    /// SessionRecorder
    /// </summary>
    public virtual long MaxMessageAutoId { get; protected set; }

    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(50)]
    [Required]
    public override string Name { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    [NotMapped]
    public virtual string DisplayName => GetDisplayName();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual string GetDisplayName()
    {
        switch (ObjectType)
        {
            case ChatObjectTypeEnums.ShopWaiter:
                {
                    return $"{FullPathName.Replace("/", ":")}";
                }
            default:
                {
                    return Name;
                }
        }
    }

    /// <summary>
    /// 拼音
    /// </summary>
    [MaxLength(300)]
    public virtual string NameSpelling { get; protected set; }

    /// <summary>
    /// 拼音简写
    /// </summary>
    [MaxLength(50)]
    public virtual string NameSpellingAbbreviation { get; protected set; }

    /// <summary>
    /// 编码
    /// </summary>
    [MaxLength(50)]
    public virtual string Code { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public virtual Genders Gender { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [NotMapped]
    public virtual string GenderDescription => Gender.GetDescription();

    /// <summary>
    /// 头像(略图)
    /// </summary>
    [MaxLength(1000)]
    public virtual string Thumbnail { get; protected set; }

    /// <summary>
    /// 头像
    /// </summary>
    [MaxLength(1000)]
    public virtual string Portrait { get; protected set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual Guid? AppUserId { get; protected set; }

    /// <summary>
    /// 对象类型
    /// </summary>
    public virtual ChatObjectTypeEnums? ObjectType { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    [NotMapped]
    public virtual string ObjectTypeDescription => ObjectType?.GetDescription();

    /// <summary>
    /// 验证方式
    /// </summary>
    public virtual VerificationMethods VerificationMethod { get; protected set; }

    /// <summary>
    /// 验证方式
    /// </summary>
    [NotMapped]
    public virtual string VerificationMethodDescription => VerificationMethod.GetDescription();

    /// <summary>
    /// 说明
    /// </summary>
    [MaxLength(500)]
    public override string Description { get; set; }

    /// <summary>
    /// 是否固有
    /// </summary>
    public virtual bool IsStatic { get; protected set; }

    /// <summary>
    /// 是否活跃
    /// </summary>
    public virtual bool IsActive { get; protected set; }

    /// <summary>
    /// 是否公开
    /// </summary>
    public virtual bool IsPublic { get; protected set; }

    /// <summary>
    /// 是否可用
    /// </summary>
    public virtual bool IsEnabled { get; protected set; } = true;

    /// <summary>
    /// default by appUserId
    /// </summary>
    public virtual bool IsDefault { get; protected set; }

    /// <summary>
    /// 客服状态
    /// </summary>
    [NotMapped]
    public virtual ServiceStatus? ServiceStatus => null;

    /// <summary>
    /// 
    /// </summary>
    [NotMapped]
    public virtual string ServiceStatusDescription => ServiceStatus?.GetDescription();

    /// <summary>
    /// 登录设备类型列表
    /// </summary>
    [NotMapped]
    public virtual List<string> DeviceTypes => null;

    /// <summary>
    /// 是否开发者
    /// </summary>
    public virtual bool IsDeveloper { get; protected set; } = false;

    #region Developer
    [InverseProperty(nameof(Developers.Developer.Owner))]
    public virtual Developer Developer { get; set; }
    #endregion

    #region Categorys
    public virtual IList<ChatObjectCategoryUnit> ChatObjectCategoryUnitList { get; set; }
    #endregion

    #region ChatObjectType
    public virtual string ChatObjectTypeId { get; protected set; }

    [ForeignKey(nameof(ChatObjectTypeId))]
    public virtual ChatObjectType ChatObjectType { get; set; }
    #endregion

    #region Message

    [InverseProperty(nameof(Message.Sender))]
    public virtual IList<Message> SenderMessageList { get; set; }

    [InverseProperty(nameof(Message.Receiver))]
    public virtual IList<Message> ReceiverMessageList { get; set; }

    #endregion

    #region MessageReminder @me,@evone

    //[InverseProperty(nameof(MessageReminder.Owner))]
    //public virtual IList<MessageReminder> MessageReminderList { get; set; }


    #endregion

    #region RedEnvelopeContent

    /// <summary>
    /// 红包发送记录
    /// </summary>
    [InverseProperty(nameof(RedEnvelopeContent.Owner))]
    public virtual IList<RedEnvelopeContent> RedEnvelopeContentList { get; set; }

    /// <summary>
    /// 红包领取记录
    /// </summary>
    [InverseProperty(nameof(RedEnvelopeUnit.Owner))]
    public virtual IList<RedEnvelopeUnit> RedEnvelopeUnitList { get; set; }

    #endregion

    #region Session

    [InverseProperty(nameof(Session.Owner))]
    public virtual IList<Session> OwnerSessionList { get; set; } = new List<Session>();

    [InverseProperty(nameof(SessionUnit.Owner))]
    public virtual IList<SessionUnit> OwnerSessionUnitList { get; set; }

    [InverseProperty(nameof(SessionUnit.Destination))]
    public virtual IList<SessionUnit> DestinationSessionUnitList { get; set; }

    //[InverseProperty(nameof(SessionUnit.Inviter))]
    //public virtual IList<SessionUnit> InviterSessionUnitList { get; set; }

    //[InverseProperty(nameof(SessionUnit.Killer))]
    //public virtual IList<SessionUnit> KillerSessionUnitList { get; set; }
    #endregion

    #region Favorite 
    //[InverseProperty(nameof(FavoritedRecorder.Owner))]
    //public virtual IList<FavoritedRecorder> FavoriteList { get; set; }
    #endregion

    #region Follow
    /// <summary>
    /// 我关注的人（我发起的关注）
    /// </summary>
    [InverseProperty(nameof(Follow.Owner))]
    public virtual IList<Follow> FollowingList { get; protected set; }

    /// <summary>
    /// 关注我的人（我是被关注者）
    /// </summary>
    [InverseProperty(nameof(Follow.Destination))]
    public virtual IList<Follow> FollowerList { get; protected set; }
    #endregion

    #region ChatObjectEntryValue 
    [InverseProperty(nameof(ChatObjectEntryValue.Owner))]
    public virtual IList<ChatObjectEntryValue> Entries { get; set; } = [];
    #endregion

    public virtual IList<ConnectionChatObject> ConnectionChatObjectList { get; protected set; } = [];

    [InverseProperty(nameof(Box.Owner))]
    public virtual IList<Box> SessionBoxList { get; protected set; } = [];

    #region Motto 
    public virtual Guid? MottoId { get; protected set; }

    [ForeignKey(nameof(MottoId))]
    public virtual Motto Motto { get; protected set; }

    [InverseProperty(nameof(Mottos.Motto.Owner))]
    public virtual IList<Motto> MottoList { get; set; }

    #endregion

    protected ChatObject()
    {
        StateCheckers = [];
    }

    public ChatObject(string name, string code, ChatObjectType chatObjectType, long? parentId) : base(name, parentId)
    {
        StateCheckers = [];
        Code = code;
        ChatObjectType = chatObjectType;
        ObjectType = chatObjectType.ObjectType;
    }

    public override void SetName(string name)
    {
        NameSpelling = name.ConvertToPinyin().MaxLength(300);
        NameSpellingAbbreviation = name.ConvertToPY().MaxLength(100);
        base.SetName(name);
    }

    public virtual void SetMaxMessageAutoId(long maxMessageAutoId)
    {
        MaxMessageAutoId = maxMessageAutoId;
    }

    public void SetPortrait(string thumbnail, string portrait)
    {
        Thumbnail = thumbnail;
        Portrait = portrait;
    }
    public void SetPortrait(string portrait)
    {
        Portrait = portrait;
    }
    public void SetThumbnail(string thumbnail)
    {
        Thumbnail = thumbnail;
    }


    public void SetMotto(Motto motto)
    {
        MottoId = motto.Id;
        Motto = motto;
    }

    public virtual void SetVerificationMethod(VerificationMethods verificationMethod)
    {
        VerificationMethod = verificationMethod;
    }

    internal void SetIsStatic(bool v)
    {
        IsStatic = v;
    }

    public void SetAppUserId(Guid appUserId)
    {
        AppUserId = appUserId;
    }
}
