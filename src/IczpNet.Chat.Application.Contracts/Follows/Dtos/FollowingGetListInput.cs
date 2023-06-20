using IczpNet.Chat.BaseDtos;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Follows.Dtos
{
    public class FollowingGetListInput : GetListInput
    {
        /// <summary>
        /// 会话单元Id
        /// </summary>
        [Required]
        public virtual Guid SessionUnitId { get; set; }

        //public bool? OwnerId { get; set; }
    }
}
