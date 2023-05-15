using IczpNet.Chat.SessionSections.SessionUnits;
using System;

namespace IczpNet.Chat.TextTemplates
{
    public class SessionUnitTextTemplate : TextTemplate
    {
        /// <summary>
        /// defaultValue:<![CDATA[<a uid="{SessionUnitId}">{ChatObjectName}</a>]]>
        /// </summary>
        public static string Template { get; set; } = "<a uid=\"{SessionUnitId}\">{ChatObjectName}</a>";

        public override string Text { get; protected set; } = Template;

        public Guid SessionUnitId { get; set; }

        public string ChatObjectName { get; set; }

        public SessionUnitTextTemplate(Guid sessionUnitId, string chatObjectName)
        {
            SessionUnitId = sessionUnitId;
            ChatObjectName = chatObjectName;
            SetData();
        }

        public SessionUnitTextTemplate(SessionUnit sessionUnit)
        {
            SessionUnitId = sessionUnit.Id;
            ChatObjectName = !sessionUnit.MemberName.IsNullOrWhiteSpace() ? sessionUnit.MemberName : sessionUnit.Owner?.Name;
            SetData();
        }

        private void SetData()
        {
            Data[nameof(SessionUnitId)] = SessionUnitId;
            Data[nameof(ChatObjectName)] = ChatObjectName;
        }

        public override string ToString()
        {
            SetData();
            return base.ToString();
        }
    }
}
