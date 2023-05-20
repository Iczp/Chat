using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageSections
{
    [ContentProvider(Name)]
    public class ArticleProvider : ContentProviderBase
    {
        public const string Name = "Article";
        protected IRepository<ArticleContent, Guid> Repository { get; set; }

        public ArticleProvider(IRepository<ArticleContent, Guid> repository)
        {
            Repository = repository;
        }

        public override Task<IContentInfo> GetContent(long messageId)
        {
            throw new NotImplementedException();
        }

        public override Task<TOutput> Create<TInput, TOutput>(TInput input)
        {
            throw new NotImplementedException();
        }
    }
}
