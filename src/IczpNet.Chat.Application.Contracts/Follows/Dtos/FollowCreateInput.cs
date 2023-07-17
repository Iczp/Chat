using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Follows.Dtos
{
    public class FollowCreateInput
    {

        /// <summary>
        /// 会话单元ID SessionUnitId
        /// </summary>
        public Guid SessionUnitId { get; set; }

        /// <summary>
        /// 要关注的 SessionUnitId
        /// </summary>
        public List<Guid> IdList { get; set; }
    }
}
