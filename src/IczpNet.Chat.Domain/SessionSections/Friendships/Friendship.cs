using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.FriendshipRequests;
using IczpNet.Chat.SessionSections.FriendshipTagUnits;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IczpNet.Chat.SessionSections.Friendships
{
    public class Friendship : BaseSessionEntity
    {
        

        public virtual Guid? RequestId { get; set; }

        [ForeignKey(nameof(RequestId))]
        public virtual FriendshipRequest FriendshipRequest { get; protected set; }

        public virtual IList<FriendshipTagUnit> FriendshipTagUnitList { get; protected set; } = new List<FriendshipTagUnit>();

        [NotMapped]
        public virtual List<FriendshipTag> TagList => GetTagList();

        /// <summary>
        /// 备注名称
        /// </summary>
        [StringLength(50)]
        public virtual string Rename { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public virtual string Remarks { get; set; }

        /// <summary>
        /// 是否保存通讯录(群)
        /// </summary>
        public virtual bool IsCantacts { get; set; }

        /// <summary>
        /// 排序,消息置顶时间，不置顶则为空
        /// </summary>
        public virtual long? SortingNumber { get; set; }

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
        public virtual bool IsShowRead { get; set; }

        /// <summary>
        /// 聊天背景，默认为 null
        /// </summary>
        [StringLength(500)]
        public virtual string BackgroundImage { get; set; }

        /// <summary>
        /// 是否被动的（主动添加为0,被动添加为1）
        /// </summary>
        public virtual bool IsPassive { get; protected set; }

        protected Friendship() { }

        public Friendship(ChatObject owner, ChatObject destination, bool isPassive, Guid? friendshipRequestId)
        {
            Owner = owner;
            Destination = destination;
            IsPassive = isPassive;
            RequestId = friendshipRequestId;
        }

        public void SetTabList(List<FriendshipTag> tagList)
        {
            FriendshipTagUnitList?.Clear();

            foreach (var tag in tagList)
            {
                FriendshipTagUnitList.Add(new FriendshipTagUnit(this, tag));
            }
        }

        public List<Guid> GetTagIdList()
        {
            return FriendshipTagUnitList?.Select(x => x.FriendshipTagId).ToList();
        }

        public List<FriendshipTag> GetTagList()
        {
            return FriendshipTagUnitList?.Select(x => x.FriendshipTag).ToList();
        }
    }
}
