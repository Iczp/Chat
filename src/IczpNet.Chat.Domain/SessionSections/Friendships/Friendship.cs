using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.Friends
{
    public class Friendship : BaseEntity<Guid>, IIsActive
    {
        //public virtual Guid? OwnerId { get; set; }

        //[ForeignKey(nameof(OwnerId))]
        //public virtual ChatObject Owner { get; set; }

        public virtual Guid FriendId { get; set; }

        [ForeignKey(nameof(FriendId))]
        public virtual ChatObject Friend { get; set; }

        [StringLength(50)]
        public virtual string Rename { get; set; }

        [StringLength(500)]
        public virtual string Remarks { get; set; }

        public virtual bool IsActive { get; set; }
    }
}
