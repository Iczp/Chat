using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IczpNet.Chat.ReadedRecorders.Dtos
{
    public class SetReadedManyInput
    {
        [Required]
        public Guid SessunitUnitId { get; set; }

        [Required]
        public List<long> MessageIdList { get; set; }

        public string DeviceId { get; set; }
    }
}
