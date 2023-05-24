using IczpNet.Chat.Words;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.TextContentWords
{
    public class TextContentWord : BaseEntity
    {
        protected TextContentWord() { }
        public virtual Guid TextContentId { get; set; }

        [ForeignKey(nameof(TextContentId))]
        public virtual TextContent TextContent { get; set; }

        public virtual Guid WordId { get; set; }

        [ForeignKey(nameof(WordId))]
        public virtual Word Word { get; set; }

        //[MaxLength(50)]
        //public virtual string Value { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { TextContentId, WordId };
        }
    }
}
