using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.TextTemplates
{
    public class SessionUnitTextTemplate : TextTemplate
    {
        /// <summary>
        /// defaultValue:<![CDATA[<a uid="{SessionUnitIds}">{ChatObjectName}</a>]]>
        /// </summary>
        public static string Template { get; set; } = "<a uid=\"{SessionUnitIds}\">{ChatObjectName}</a>";

        public override string Text { get; protected set; } = Template;

        public List<Guid> SessionUnitIds { get; set; }

        public string ChatObjectName { get; set; }

        public SessionUnitTextTemplate(List<Guid> sessionUnitIds, string chatObjectName)
        {
            SessionUnitIds = sessionUnitIds;
            ChatObjectName = chatObjectName;
            SetData();
        }

        public SessionUnitTextTemplate(Guid sessionUnitId, string chatObjectName)
        {
            SessionUnitIds = new List<Guid>() { sessionUnitId };
            ChatObjectName = chatObjectName;
            SetData();
        }

        public SessionUnitTextTemplate(SessionUnit sessionUnit)
        {
            SessionUnitIds = new List<Guid>() { sessionUnit.Id };
            ChatObjectName = !sessionUnit.Setting.MemberName.IsNullOrWhiteSpace() ? sessionUnit.Setting.MemberName : sessionUnit.Owner?.Name;
            SetData();
        }

        private void SetData()
        {
            Data[nameof(SessionUnitIds)] = SessionUnitIds.JoinAsString(",");
            Data[nameof(ChatObjectName)] = ChatObjectName;
        }

        public override string ToString()
        {
            SetData();
            return base.ToString();
        }
    }
}
