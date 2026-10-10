using IczpNet.Chat.MessageSections.Templates;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

// The legacy UsingMessageTemplate convention leaves identity and audit state
// untouched on input. Mapperly only writes public destination setters here.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainCmdContentInfoToCmdContentMapper
    : TwoWayMapperBase<CmdContentInfo, CmdContent>
{
    public override partial CmdContent Map(CmdContentInfo source);

    public override partial void Map(CmdContentInfo source, CmdContent destination);

    public override partial CmdContentInfo ReverseMap(CmdContent source);

    public override partial void ReverseMap(CmdContent source, CmdContentInfo destination);
}
