using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.SessionSections.FriendshipTagUnits;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.Friendships
{
    public class FriendshipTag : BaseEntity<Guid>, IChatOwner<Guid?>
    {
        public virtual Guid? OwnerId { get; protected set; }

        [ForeignKey(nameof(OwnerId))]
        public virtual ChatObject Owner { get; protected set; }

        [StringLength(20)]
        public virtual string Name { get; set; }

        public virtual IList<FriendshipTagUnit> FriendshipList { get; protected set; }

    }
}
