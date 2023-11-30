using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.SessionTags;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public interface ISessionUnitSenderInfo
    {
        Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// 会话内名称（如：群内名称）
        /// </summary>
        string MemberName { get; set; }

        //Guid SessionId { get; set; }

        //long OwnerId { get; set; }

        IChatObject Owner { get; set; }

        //bool IsPublic { get; set; }

        //bool IsStatic { get; set; }

        bool IsCreator { get; set; }

    }
}
