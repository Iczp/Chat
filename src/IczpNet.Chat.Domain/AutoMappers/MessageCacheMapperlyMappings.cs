using IczpNet.Chat.MessageSections.Messages;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

// MessageQuoteCacheItem intentionally has no nested quote member. This keeps
// the cached quote to one level, as in the existing AutoMapper DTO shape.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainMessageToMessageQuoteCacheItemMapper
    : MapperBase<Message, MessageQuoteCacheItem>
{
    public override MessageQuoteCacheItem Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageQuoteCacheItem();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageQuoteCacheItem destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Content = source.Content;
    }

    [MapperIgnoreTarget(nameof(MessageQuoteCacheItem.Content))]
    private partial void MapFields(Message source, MessageQuoteCacheItem destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainMessageToMessageCacheItemMapper
    : MapperBase<Message, MessageCacheItem>
{
    private readonly ChatDomainMessageToMessageQuoteCacheItemMapper _quoteMapper = new();

    public override MessageCacheItem Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageCacheItem();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageCacheItem destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Content = source.Content;
        destination.QuoteMessage = source.QuoteMessage is null
            ? null!
            : _quoteMapper.Map(source.QuoteMessage);
    }

    [MapperIgnoreTarget(nameof(MessageCacheItem.QuoteMessage))]
    [MapperIgnoreTarget(nameof(MessageCacheItem.Content))]
    private partial void MapFields(Message source, MessageCacheItem destination);
}
