using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates
{
    [MessageTemplate(MessageTypes.Text)]
    [ContentOuput(typeof(TextContentInfo))]
    public class TextContent : MessageContentEntityBase
    {
        [StringLength(500)]
        public virtual string Text { get; set; }

        public override string GetBody() => FormatString(Text);

        public override long GetSize() => System.Text.Encoding.Default.GetByteCount(Text);
    }
}
