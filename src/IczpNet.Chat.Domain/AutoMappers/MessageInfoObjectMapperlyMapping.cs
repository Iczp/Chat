using IczpNet.Chat.MessageSections.Messages;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

// This concrete closed mapping is used by message jobs. The legacy open-generic
// mapping still needs separate treatment for MessageInfo<TContent> callers.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainMessageToMessageInfoObjectMapper
    : MapperBase<Message, MessageInfo<object>>
{
    public override MessageInfo<object> Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageInfo<object>();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageInfo<object> destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Content = source.Content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<object>.Content))]
    private partial void MapFields(Message source, MessageInfo<object> destination);
}
