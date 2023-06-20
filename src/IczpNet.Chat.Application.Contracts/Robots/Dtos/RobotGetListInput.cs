using IczpNet.Chat.BaseDtos;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace IczpNet.Chat.Robots.Dtos
{
    public class RobotGetListInput : BaseGetListInput
    {
        /// <summary>
        /// 是否可用
        /// </summary>
        public virtual bool? IsEnabled { get; set; }

        /// <summary>
        /// 目录Id
        /// </summary>
        [DefaultValue(null)]
        public virtual List<Guid> CategoryIdList { get; set; }

        /// <summary>
        /// 包含下级
        /// </summary>
        [DefaultValue(null)]
        public bool? IsImportChildCategory { get; set; }
    }
}
