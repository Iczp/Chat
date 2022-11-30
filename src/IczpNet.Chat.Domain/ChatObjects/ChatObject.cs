using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Messages;
using IczpNet.Chat.OfficialSections.OfficialExcludedMembers;
using IczpNet.Chat.OfficialSections.OfficialGroupMembers;
using IczpNet.Chat.OfficialSections.OfficialMembers;
using IczpNet.Chat.Robots;
using IczpNet.Chat.RoomSections.RoomMembers;
using IczpNet.Chat.SessionSections.Friends;
using IczpNet.Chat.SquareSections.SquareMembers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.ChatObjects
{
    //[Index]
    public class ChatObject : BaseEntity<Guid>, IChatObject
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long AutoId { get; set; }

        [StringLength(50)]
        [Required]
        public virtual string Name { get; set; }

        [StringLength(50)]
        public virtual string Code { get; set; }

        public virtual Guid? OwnerUserId { get; set; }

        public virtual ChatObjectTypeEnum? ChatObjectType { get; protected set; }

        [StringLength(500)]
        public virtual string Description { get; set; }

        #region Message

        [InverseProperty(nameof(Message.Sender))]
        public virtual IList<Message> SenderMessageList { get; set; }

        [InverseProperty(nameof(Message.Receiver))]
        public virtual IList<Message> ReceiverMessageList { get; set; }

        #endregion

        #region ShopKeeper
        /// <summary>
        /// 兼职店小二
        /// </summary>
        [InverseProperty(nameof(ShopWaiter.ChatObject))]
        public virtual IList<ShopWaiter> ProxyShopWaiterList { get; set; }

        /// <summary>
        /// 兼职掌柜
        /// </summary>
        [InverseProperty(nameof(ShopKeeper.ChatObject))]
        public virtual IList<ShopKeeper> ProxyShopKeeperList { get; set; }

        #endregion

        #region RoomMember

        [InverseProperty(nameof(RoomMember.Owner))]
        public virtual IList<RoomMember> InRoomMemberList { get; set; }

        #endregion

        #region Official

        [InverseProperty(nameof(OfficialGroupMember.ChatObject))]
        public virtual IList<OfficialGroupMember> InOfficialGroupMemberList { get; set; }

        [InverseProperty(nameof(OfficialMember.ChatObject))]
        public virtual IList<OfficialMember> InOfficialMemberList { get; set; }

        [InverseProperty(nameof(OfficalExcludedMember.ChatObject))]
        public virtual IList<OfficalExcludedMember> InOfficalExcludedMemberList { get; set; }

        #endregion

        #region SquareMember

        [InverseProperty(nameof(SquareMember.Owner))]
        public virtual IList<SquareMember> InSquareMemberList { get; set; }

        #endregion

        #region Friendship

        [InverseProperty(nameof(Friendship.Owner))]
        public virtual IList<Friendship> OwnerFriendList { get; set; }

        [InverseProperty(nameof(Friendship.Friend))]
        public virtual IList<Friendship> DestinationFriendList { get; set; }

        #endregion

        #region SessionSetting

        //[InverseProperty(nameof(SessionSetting.Owner))]
        //public virtual IList<SessionSetting> OwnerSessionSettingList { get; set; }

        //[InverseProperty(nameof(SessionSetting.Destination))]
        //public virtual IList<SessionSetting> DestinationSessionSettingList { get; set; }

        #endregion

        protected ChatObject() { }

        protected ChatObject(Guid id, ChatObjectTypeEnum chatObjectType) : base(id)
        {
            ChatObjectType = chatObjectType;
        }
    }
}
