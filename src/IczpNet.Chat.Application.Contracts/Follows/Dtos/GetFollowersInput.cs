using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Follows.Dtos
{
    public class GetFollowersInput : BaseGetListInput
    {

        /// <summary>
        /// SessionUnitId
        /// </summary>
        public Guid DestinationId { get; set; }
    }
}
