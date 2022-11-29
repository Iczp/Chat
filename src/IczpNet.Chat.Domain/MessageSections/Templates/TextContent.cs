using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates
{
    public class TextContent : MessageContent
    {
        [StringLength(500)]
        public virtual string Text { get; set; }
    }
}
