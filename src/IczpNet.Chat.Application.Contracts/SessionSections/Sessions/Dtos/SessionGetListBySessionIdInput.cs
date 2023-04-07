using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionGetListBySessionIdInput : PagedAndSortedResultRequestDto, IKeyword
    {
        [Required]
        public virtual Guid SessionId { get; set; }

        public virtual bool? IsKilled { get; set; }

        public virtual bool? IsPublic { get; set; }

        public virtual bool? IsStatic { get; set; }

        public virtual List<long> OwnerIdList { get; set; }

        [DefaultValue(null)]
        public virtual List<ChatObjectTypeEnums> OwnerTypeList { get; set; }

        public virtual Guid? TagId { get; set; }

        public virtual Guid? RoleId { get; set; }

        public virtual JoinWays? JoinWay { get; set; }

        public virtual long? InviterId { get; set; }

        public virtual Guid? InviterUnitId { get; set; }

        public virtual string Keyword { get; set; }

    }
}