using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.ContactTags.Dtos;
using IczpNet.Chat.SessionUnits.Dtos;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Cantacts.Dtos
{
    public class ContactsDto
    {
        /// <summary>
        /// 会话单元Id
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// 聊天对象
        /// </summary>
        public virtual ChatObjectDto Destination { get; set; }

        public virtual SessionUnitSettingDto Setting { get; set; }

        public virtual List<ContactTagSimpleDto> ContactTags { get; set; }

    }
}
