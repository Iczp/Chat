using Castle.Components.DictionaryAdapter;
using IczpNet.Chat.BaseEntitys;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.HttpRequests
{
    public class HttpResponse : BaseEntity
    {
        public virtual Guid HttpRequestId { get; set; }

        [ForeignKey(nameof(HttpRequestId))]
        public virtual HttpRequest HttpRequest { get; set; }

        public virtual string Content { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { HttpRequestId };
        }
    }
}
