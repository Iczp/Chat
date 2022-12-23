using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitGetSessionMemberListInput : PagedAndSortedResultRequestDto
    {
        public virtual Guid? TagId { get; set; }

        public virtual Guid? RoleId { get; set; }
    }
}
