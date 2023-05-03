using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Follows.Dtos
{
    public class FollowGetListInput : BaseGetListInput
    {

        /// <summary>
        /// SessionUnitId
        /// </summary>
        public Guid OwnerId { get; set; }
    }
}
