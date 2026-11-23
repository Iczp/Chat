using IczpNet.Chat.MessageTemplates;
using IczpNet.Chat.MessageTemplates.Dtos;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationMessageTemplateDefinitionToDtoMapper : MapperBase<MessageTemplateDefinition, MessageTemplateDto>
{
    public override partial MessageTemplateDto Map(MessageTemplateDefinition source);
    public override partial void Map(MessageTemplateDefinition source, MessageTemplateDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationMessageTemplateDefinitionToDetailDtoMapper : MapperBase<MessageTemplateDefinition, MessageTemplateDetailDto>
{
    public override partial MessageTemplateDetailDto Map(MessageTemplateDefinition source);
    public override partial void Map(MessageTemplateDefinition source, MessageTemplateDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationMessageTemplateRevisionToDtoMapper : MapperBase<MessageTemplateRevision, MessageTemplateRevisionDto>
{
    public override partial MessageTemplateRevisionDto Map(MessageTemplateRevision source);
    public override partial void Map(MessageTemplateRevision source, MessageTemplateRevisionDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationMessageTemplateRevisionToDetailDtoMapper : MapperBase<MessageTemplateRevision, MessageTemplateRevisionDetailDto>
{
    public override partial MessageTemplateRevisionDetailDto Map(MessageTemplateRevision source);
    public override partial void Map(MessageTemplateRevision source, MessageTemplateRevisionDetailDto destination);
}
