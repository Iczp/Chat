using IczpNet.Chat.MessageSections.Templates;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageSections
{
    [ContentProvider(Name)]
    public class TextContentProvider : ContentProviderBase
    {
        public const string Name = "Text";
        protected IRepository<TextContent, Guid> Repository { get; set; }

        public TextContentProvider(IRepository<TextContent, Guid> repository)
        {
            Repository = repository;
        }

        public override async Task<IMessageContentInfo> GetContent(Guid messageId)
        {
            var content = await Repository.FindAsync(x => x.MessageList.Any(d => d.Id == messageId));

            if (content == null)
            {
                Logger.LogWarning($"No such TextContent by messageId:{messageId}");
                return null;
            }
            return ObjectMapper.Map<TextContent, TextContentInfo>(content);
        }

        public override Task<TOutput> Create<TInput, TOutput>(TInput input)
        {
            return Task.FromResult(ObjectMapper.Map<TInput, TOutput>(input));
        }
    }
}
