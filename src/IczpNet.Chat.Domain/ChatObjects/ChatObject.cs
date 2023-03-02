using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjectCategoryUnits;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.SessionSections.Favorites;
using IczpNet.Chat.SessionSections.FriendshipRequests;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionSettings;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.SimpleStateChecking;

namespace IczpNet.Chat.ChatObjects
{
    //[Index]
    public class ChatObject : BaseTreeEntity<ChatObject, long>, IName, IChatObject, IHasSimpleStateCheckers<ChatObject>, IIsStatic, IIsActive
    {
        public List<ISimpleStateChecker<ChatObject>> StateCheckers { get; }

        [StringLength(50)]
        public virtual string TypeName { get; private set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long AutoId { get; }

        /// <summary>
        /// SessionRecorder
        /// </summary>
        public virtual long MaxMessageAutoId { get; protected set; }

        [StringLength(50)]
        [Required]
        public override string Name { get; protected set; }

        [StringLength(50)]
        public virtual string Code { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [StringLength(300)]
        [MaxLength(300)]
        public virtual string Portrait { get; protected set; }

        public virtual Guid? AppUserId { get; protected set; }

        public virtual ChatObjectTypeEnums? ObjectType { get; protected set; }

        [StringLength(500)]
        public override string Description { get; set; }

        public virtual bool IsStatic { get; set; }

        public virtual bool IsActive { get; set; }

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

        [InverseProperty(nameof(SessionUnit.Inviter))]
        public virtual IList<SessionUnit> InviterSessionUnitList { get; set; }

        [InverseProperty(nameof(SessionUnit.Killer))]
        public virtual IList<SessionUnit> KillerSessionUnitList { get; set; }
        #endregion

        #region Favorite 
        [InverseProperty(nameof(Favorite.Owner))]
        public virtual IList<Favorite> FavoriteList { get; set; }
        #endregion


        protected ChatObject()
        {
            StateCheckers = new List<ISimpleStateChecker<ChatObject>>();
            TypeName = GetType().Name;
        }

        public ChatObject(string name, ChatObjectType chatObjectType, long? parentId) : base(name, parentId)
        {
            TypeName = GetType().Name;
            StateCheckers = new List<ISimpleStateChecker<ChatObject>>();
            ChatObjectType = chatObjectType;
        }

        public override void SetName(string name)
        {
            base.SetName(name);
        }

        public virtual void SetMaxMessageAutoId(long maxMessageAutoId)
        {
            MaxMessageAutoId = maxMessageAutoId;
        }
    }
}
