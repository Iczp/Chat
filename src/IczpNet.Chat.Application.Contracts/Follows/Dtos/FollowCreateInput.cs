using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Follows.Dtos
{
    public class FollowCreateInput
    {

        /// <summary>
        /// SessionUnitId
        /// </summary>
        public Guid OwnerId { get; set; }

        public List<Guid> IdList { get; set; }
    }
}
