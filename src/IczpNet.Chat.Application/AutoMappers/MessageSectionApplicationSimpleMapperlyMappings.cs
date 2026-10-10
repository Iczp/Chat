// Generated from conventional CreateMap declarations. Keep complex maps in the profile until migrated.
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class MessageSectionApplicationMessageToMessageSimpleDtoMapper : MapperBase<Message, MessageSimpleDto>
{
    public override partial MessageSimpleDto Map(Message source);

    public override partial void Map(Message source, MessageSimpleDto destination);
}
