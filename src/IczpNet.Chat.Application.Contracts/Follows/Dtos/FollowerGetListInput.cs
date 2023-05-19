using IczpNet.Chat.BaseDtos;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Follows.Dtos
{
    public class FollowerGetListInput : BaseGetListInput
    {

        [Required]
        public Guid SessionUnitId { get; set; }

        //public long? DestinationId { get; set; }
    }
}
