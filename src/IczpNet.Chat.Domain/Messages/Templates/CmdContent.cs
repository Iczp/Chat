using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Messages.Templates
{
    public class CmdContent : MessageContent
    {
        [StringLength(20)]
        public virtual string Cmd { get; set; }

        [StringLength(500)]
        public virtual string Text { get; set; }
    }
}
