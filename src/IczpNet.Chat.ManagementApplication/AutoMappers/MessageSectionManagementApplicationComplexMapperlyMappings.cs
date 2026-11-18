using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.Management.MessageSections.Messages.Dtos;
using IczpNet.Chat.MessageSections.Messages;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.Management.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationMessageToMessageDtoMapper
    : MapperBase<Message, MessageDto>
{
    public override partial MessageDto Map(Message source);
    public override partial void Map(Message source, MessageDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationMessageToMessageDetailDtoMapper
    : MapperBase<Message, MessageDetailDto>
{
    public override partial MessageDetailDto Map(Message source);
    public override partial void Map(Message source, MessageDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationMessageCreateInputToMessageMapper
    : MapperBase<MessageCreateInput, Message>
{
    public override Message Map(MessageCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<Message>();
        Map(source, destination);
        return destination;
    }

    public override void Map(MessageCreateInput source, Message destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(MessageCreateInput source, Message destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationMessageUpdateInputToMessageMapper
    : MapperBase<MessageUpdateInput, Message>
{
    public override Message Map(MessageUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<Message>();
        Map(source, destination);
        return destination;
    }

    public override void Map(MessageUpdateInput source, Message destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(MessageUpdateInput source, Message destination);
}
