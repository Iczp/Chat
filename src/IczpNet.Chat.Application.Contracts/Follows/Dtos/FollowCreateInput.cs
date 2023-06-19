using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Follows.Dtos
{
    public class FollowCreateInput
    {

        /// <summary>
        /// 会话单元ID SessionUnitId
        /// </summary>
        public Guid OwnerId { get; set; }

        public List<Guid> IdList { get; set; }
    }
}
