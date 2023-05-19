using IczpNet.Chat.BaseDtos;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Follows.Dtos
{
    public class FollowingGetListInput : BaseGetListInput
    {
        [Required]
        public Guid SessionUnitId { get; set; }

        //public bool? OwnerId { get; set; }
    }
}
