using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionSetUnitGetListInput : PagedAndSortedResultRequestDto, IKeyword
    {
        public virtual Guid SessionId { get; set; }

        public virtual Guid? TagId { get; set; }

        public virtual Guid? RoleId { get; set; }

        public virtual JoinWays? JoinWay { get; set; }

        public virtual Guid? InviterId { get; set; }

        public virtual string Keyword { get; set; }

    }
}