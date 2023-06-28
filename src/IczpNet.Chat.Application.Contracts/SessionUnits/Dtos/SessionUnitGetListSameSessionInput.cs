using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionUnits.Dtos
{
    public class SessionUnitGetListSameSessionInput : GetListInput
    {
        /// <summary>
        /// 源 聊天对象
        /// </summary>
        [Required]
        public virtual long SourceId { get; set; }

        /// <summary>
        /// 目标聊天对象
        /// </summary>
        [Required] 
        public virtual long TargetId { get; set; }

        /// <summary>
        /// 聊天对象类型
        /// </summary>
        [DefaultValue(null)]
        public virtual List<ChatObjectTypeEnums> ObjectTypeList { get; set; }

    }
}