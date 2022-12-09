using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates
{
    [MessageTemplate(MessageTypes.Text)]
    public class TextContent : BaseMessageContentEntity
    {
        //protected TextContent() { }

        //public TextContent(Guid id) : base(id)
        //{
        //}

        [StringLength(500)]
        public virtual string Text { get; set; }
    }
}
