using IczpNet.Chat.MessageSections.Messages;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

// MessageInfo<dynamic> is MessageInfo<object> at runtime; register one mapper
// for both forms. Other active content types have their own closed mappers.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainMessageToMessageInfoObjectMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<object>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

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
