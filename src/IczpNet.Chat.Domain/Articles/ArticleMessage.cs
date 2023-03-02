using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Articles
{
    public class ArticleMessage : BaseEntity
    {
        public virtual Guid ArticleId { get; protected set; }

        [ForeignKey(nameof(ArticleId))]
        public virtual Article Article { get; protected set; }

        public virtual long MessageId { get; protected set; }

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; protected set; }

        protected ArticleMessage() { }

        //public ArticleMessage(Article article, Message message)
        //{
        //    Article = article;
        //    Message = message;
        //}

        public ArticleMessage(Guid articleId, long messageId)
        {
            ArticleId = articleId;
            MessageId = messageId;
        }
        public ArticleMessage(Message message)
        {
            Message = message;
        }

        public override object[] GetKeys()
        {
            return new object[] { ArticleId, MessageId };
        }
    }
}
