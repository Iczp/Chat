using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitCacheGetListInput : PagedAndSortedResultRequestDto
    {
        /// <summary>
        /// 会话Id（二者不能同时为null）
        /// </summary>
        public Guid? SessionId { get; set; }

        /// <summary>
        /// 会话单元Id（二者不能同时为null）
        /// </summary>
        public Guid? SessionUnitId { get; set; }
    }
}
