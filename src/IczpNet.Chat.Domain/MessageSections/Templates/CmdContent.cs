using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates
{
    [MessageTemplate(MessageTypes.Cmd)]
    public class CmdContent : BaseMessageContentEntity
    {
        [StringLength(20)]
        public virtual string Cmd { get; set; }

        [StringLength(500)]
        public virtual string Text { get; set; }

        [StringLength(500)]
        /// <summary>
        /// app:///pages/im/notice?id=123
        /// </summary>
        public virtual string Url { get; internal set; }
    }
}
