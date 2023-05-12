using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitCacheGetListInput : PagedAndSortedResultRequestDto
    {
        public Guid? SessionId { get; set; }

        public Guid? SessionUnitId { get; set; }
    }
}
