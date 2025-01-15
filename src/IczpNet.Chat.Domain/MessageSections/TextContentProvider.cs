using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Templates;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageSections;

[ContentProvider(MessageTypes.Text)]
public class TextContentProvider : ContentProviderBase<TextContent>
{
    public const string Name = "Text";

    public override string ProviderName => Name;

    public TextContentProvider(IRepository<TextContent, Guid> repository) : base(repository)
    {

    }

    public override async Task<IContentInfo> GetContentInfoAsync(long messageId)
    {
        var content = await Repository.FirstOrDefaultAsync(x => x.MessageList.Any(d => d.Id == messageId));

        if (content == null)
        {
            Logger.LogWarning($"No such TextContent by messageId:{messageId}");
            return null;
        }
        return ObjectMapper.Map<TextContent, TextContentInfo>(content);
    }

    public override Task<TOutput> CreateAsync<TInput, TOutput>(TInput input)
    {
        return Task.FromResult(ObjectMapper.Map<TInput, TOutput>(input));
    }
}
