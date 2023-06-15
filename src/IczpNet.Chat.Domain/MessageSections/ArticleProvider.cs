using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Templates;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageSections
{
    [ContentProvider(MessageTypes.Article)]
    public class ArticleProvider : ContentProviderBase<ArticleContent>
    {
        public const string Name = "Article";

        public override string ProviderName => Name;

        public ArticleProvider(IRepository<ArticleContent, Guid> repository) : base(repository)
        {

        }

        public override Task<IContentInfo> GetContentInfoAsync(long messageId)
        {
            throw new NotImplementedException();
        }

        public override Task<TOutput> CreateAsync<TInput, TOutput>(TInput input)
        {
            throw new NotImplementedException();
        }
    }
}
