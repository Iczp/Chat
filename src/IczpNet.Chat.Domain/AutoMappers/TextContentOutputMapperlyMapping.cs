using IczpNet.Chat.MessageSections.Templates;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

// The input direction remains TextContentInfoMapper, which performs text
// validation. This mapping is only for the persisted entity -> client DTO.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainTextContentToTextContentInfoMapper
    : MapperBase<TextContent, TextContentInfo>
{
    public override partial TextContentInfo Map(TextContent source);

    public override partial void Map(TextContent source, TextContentInfo destination);
}
