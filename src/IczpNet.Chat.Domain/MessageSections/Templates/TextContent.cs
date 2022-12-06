using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates
{
    public class TextContent : BaseMessageContentEntity
    {
        [StringLength(500)]
        public virtual string Text { get; set; }
    }
}
