using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates
{
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
