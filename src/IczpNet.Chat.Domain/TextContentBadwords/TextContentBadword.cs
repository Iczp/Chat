using IczpNet.Chat.Badwords;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.TextContentBadwords
{
    public class TextContentBadword //: BaseEntity
    {
        protected TextContentBadword() { }
        public virtual Guid TextContentId { get; set; }

        [ForeignKey(nameof(TextContentId))]
        public virtual TextContent TextContent { get; set; }

        public virtual Guid BadwordId { get; set; }

        [ForeignKey(nameof(BadwordId))]
        public virtual Badword Badword { get; set; }

        //[MaxLength(50)]
        //public virtual string Badword { get; set; }

        //public override object[] GetKeys()
        //{
        //    return new object[] { TextContentId, BadwordId };
        //}
    }
}
