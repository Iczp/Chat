using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.OfficialSections.OfficialExcludedMembers;
using IczpNet.Chat.OfficialSections.OfficialGroupMembers;
using IczpNet.Chat.OfficialSections.OfficialMembers;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.Robots;
using IczpNet.Chat.RoomSections.RoomForbiddenMembers;
using IczpNet.Chat.RoomSections.RoomMembers;
using IczpNet.Chat.SessionSections.Favorites;
using IczpNet.Chat.SessionSections.FriendshipRequests;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.MessageReminders;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionSettings;
using IczpNet.Chat.SquareSections.SquareMembers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.SimpleStateChecking;

namespace IczpNet.Chat.ChatObjects
{
    //[Index]
    public class ChatObject : BaseEntity<Guid>, IName, IChatObject, IHasSimpleStateCheckers<ChatObject>
    {
        public List<ISimpleStateChecker<ChatObject>> StateCheckers { get; }

        [StringLength(50)]
        public virtual string TypeName { get; private set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long AutoId { get; }

        [StringLength(50)]
        [Required]
        public virtual string Name { get; protected set; }

        [StringLength(50)]
        public virtual string Code { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [StringLength(300)]
        [MaxLength(300)]
        public virtual string Portrait { get; protected set; }

        public virtual Guid? AppUserId { get; protected set; }

        public virtual ChatObjectTypes? ObjectType { get; protected set; }

        [StringLength(500)]
        public virtual string Description { get; set; }

        #region Message

        [InverseProperty(nameof(Message.Sender))]
        public virtual IList<Message> SenderMessageList { get; set; }

        [InverseProperty(nameof(Message.Receiver))]
        public virtual IList<Message> ReceiverMessageList { get; set; }

        #endregion

        #region MessageReminder @me,@evone

        [InverseProperty(nameof(MessageReminder.Owner))]
        public virtual IList<MessageReminder> MessageReminderList { get; set; }


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

        #region Shop
        /// <summary>
        /// 兼职店小二
        /// </summary>
        [InverseProperty(nameof(ShopWaiter.Owner))]
        public virtual IList<ShopWaiter> ProxyShopWaiterList { get; set; }

        /// <summary>
        /// 兼职掌柜
        /// </summary>
        [InverseProperty(nameof(ShopKeeper.Owner))]
        public virtual IList<ShopKeeper> ProxyShopKeeperList { get; set; }

        #endregion

        #region Room

        [InverseProperty(nameof(RoomMember.Owner))]
        public virtual IList<RoomMember> InRoomMemberList { get; set; }

        [InverseProperty(nameof(RoomForbiddenMember.Owner))]
        public virtual IList<RoomForbiddenMember> InRoomForbiddenMemberList { get; set; }

        [InverseProperty(nameof(RoomMember.Inviter))]
        public virtual IList<RoomMember> InInviterList { get; set; }

        #endregion

        #region Official

        [InverseProperty(nameof(OfficialGroupMember.Owner))]
        public virtual IList<OfficialGroupMember> InOfficialGroupMemberList { get; set; }

        [InverseProperty(nameof(OfficialMember.Owner))]
        public virtual IList<OfficialMember> InOfficialMemberList { get; set; }

        [InverseProperty(nameof(OfficalExcludedMember.Owner))]
        public virtual IList<OfficalExcludedMember> InOfficalExcludedMemberList { get; set; }

        #endregion

        #region Square

        [InverseProperty(nameof(SquareMember.Owner))]
        public virtual IList<SquareMember> InSquareMemberList { get; set; }

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
        public virtual IList<Session> OwnerSessionList { get; set; }

        [InverseProperty(nameof(Session.Destination))]
        public virtual IList<Session> DestinationSessionList { get; set; }

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

        protected ChatObject(Guid id, ChatObjectTypes chatObjectType) : base(id)
        {
            ObjectType = chatObjectType;
            TypeName = GetType().Name;
            StateCheckers = new List<ISimpleStateChecker<ChatObject>>();
        }

        public virtual void SetName(string name)
        {
            Name = name;
        }
    }
}
