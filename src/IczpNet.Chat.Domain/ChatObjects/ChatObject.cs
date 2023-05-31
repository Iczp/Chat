using IczpNet.AbpCommons.DataFilters;
using IczpNet.AbpCommons.PinYin;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjectCategoryUnits;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Mottos;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.SessionSections.FriendshipRequests;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionSettings;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.SimpleStateChecking;
using IczpNet.Chat.Developers;

namespace IczpNet.Chat.ChatObjects
{
    [Index(nameof(Code))]
    [Index(nameof(Name))]
    [Index(nameof(FullPath))]
    public class ChatObject : BaseTreeEntity<ChatObject, long>, IName, IChatObject, IHasSimpleStateCheckers<ChatObject>, IIsStatic, IIsActive, IIsPublic, IIsEnabled, IIsDefault
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

        [MaxLength(50)]
        [Required]
        public override string Name { get; protected set; }


        [MaxLength(300)]
        public virtual string NameSpelling { get; protected set; }

        [MaxLength(50)]
        public virtual string NameSpellingAbbreviation { get; protected set; }

        [MaxLength(50)]
        public virtual string Code { get; set; }

        public virtual Genders Gender { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [MaxLength(1000)]
        public virtual string Portrait { get; protected set; }

        public virtual Guid? AppUserId { get; protected set; }

        public virtual ChatObjectTypeEnums? ObjectType { get; protected set; }

        public virtual VerificationMethods VerificationMethod { get; protected set; }

        [MaxLength(500)]
        public override string Description { get; set; }

        public virtual bool IsStatic { get; protected set; }

        public virtual bool IsActive { get; protected set; }

        public virtual bool IsPublic { get; protected set; }

        public virtual bool IsEnabled { get; protected set; } = true;

        public virtual bool IsDefault { get; protected set; }

        /// <summary>
        /// 客服状态
        /// </summary>
        public virtual ServiceStatus? ServiceStatus { get; protected set; }

        public virtual bool IsDeveloper { get; protected set; } = false;

        [InverseProperty(nameof(Developers.Developer.Owner))]
        public virtual Developer Developer { get;  set; }

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

        #region Friendship

        [InverseProperty(nameof(Friendship.Owner))]
        public virtual IList<Friendship> OwnerFriendshipList { get; set; }

        [InverseProperty(nameof(Friendship.Destination))]
        public virtual IList<Friendship> DestinationFriendshipList { get; set; }

        #endregion FriendshipRequest

        #region FriendshipRequest
        [InverseProperty(nameof(FriendshipRequest.Owner))]
        public virtual IList<FriendshipRequest> OwnerFriendshipRequestList { get; set; }

        [InverseProperty(nameof(FriendshipRequest.Destination))]
        public virtual IList<FriendshipRequest> DestinationFriendshipRequestList { get; set; }
        #endregion

        #region SessionSetting

        [InverseProperty(nameof(SessionSetting.Owner))]
        public virtual IList<SessionSetting> OwnerSessionSettingList { get; set; }

        [InverseProperty(nameof(SessionSetting.Destination))]
        public virtual IList<SessionSetting> DestinationSessionSettingList { get; set; }

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


        #region Motto 
        public virtual Guid? MottoId { get; protected set; }

        [ForeignKey(nameof(MottoId))]
        public virtual Motto Motto { get; protected set; }

        [InverseProperty(nameof(Mottos.Motto.Owner))]
        public virtual IList<Motto> MottoList { get; set; }

        #endregion

        protected ChatObject()
        {
            StateCheckers = new List<ISimpleStateChecker<ChatObject>>();
        }

        public ChatObject(string name, ChatObjectType chatObjectType, long? parentId) : base(name, parentId)
        {
            StateCheckers = new List<ISimpleStateChecker<ChatObject>>();
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

        public void SetPortrait(string portrait)
        {
            Portrait = portrait;
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

        internal void BingAppUserId(Guid appUserId)
        {
            AppUserId = appUserId;
        }
    }
}
