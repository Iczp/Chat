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
    public override partial MessageInfo<object> Map(Message source);

    public override partial void Map(Message source, MessageInfo<object> destination);
}
