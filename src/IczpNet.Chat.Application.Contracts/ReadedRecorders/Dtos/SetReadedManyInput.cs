using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.ReadedRecorders.Dtos
{
    public class SetReadedManyInput
    {
        /// <summary>
        /// 会话单元Id
        /// </summary>
        [Required]
        public Guid SessunitUnitId { get; set; }

        /// <summary>
        /// 消息列表
        /// </summary>
        [Required]
        public List<long> MessageIdList { get; set; }

        /// <summary>
        /// 设备Id
        /// </summary>
        public string DeviceId { get; set; }
    }
}
