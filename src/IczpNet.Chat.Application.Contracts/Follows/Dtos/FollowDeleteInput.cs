using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Follows.Dtos
{
    public class FollowDeleteInput
    {

        /// <summary>
        /// 会话单元ID SessionUnitId
        /// </summary>
        public Guid SessionUnitId { get; set; }

        /// <summary>
        /// 要取消关注的 SessionUnitId
        /// </summary>
        public List<Guid> IdList { get; set; }
    }
}
