// Generated from conventional CreateMap declarations. Keep complex maps in the profile until migrated.
using IczpNet.AbpTrees;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ConnectionPools;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.SessionBoxes;
using IczpNet.Chat.SessionTags;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.UrlNormalizers;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainHistoryContentToHistoryContentOutputMapper : MapperBase<HistoryContent, HistoryContentOutput>
{
    public override partial HistoryContentOutput Map(HistoryContent source);

    public override partial void Map(HistoryContent source, HistoryContentOutput destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainSessionUnitToSessionUnitSenderInfoMapper(ChatDomainChatObjectToChatObjectInfoMapper chatObjectMapper) : MapperBase<SessionUnit, SessionUnitSenderInfo>
{
    [UseMapper]
    private readonly ChatDomainChatObjectToChatObjectInfoMapper _chatObjectMapper = chatObjectMapper;

    public override partial SessionUnitSenderInfo Map(SessionUnit source);

    public override partial void Map(SessionUnit source, SessionUnitSenderInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainSessionUnitToSessionUnitCacheItemMapper : MapperBase<SessionUnit, SessionUnitCacheItem>
{
    public override partial SessionUnitCacheItem Map(SessionUnit source);

    public override partial void Map(SessionUnit source, SessionUnitCacheItem destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainSessionTagToSessionTagCacheItemMapper : MapperBase<SessionTag, SessionTagCacheItem>
{
    public override partial SessionTagCacheItem Map(SessionTag source);

    public override partial void Map(SessionTag source, SessionTagCacheItem destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainConnectionPoolCacheItemToDisconnectedEtoMapper : TwoWayMapperBase<ConnectionPoolCacheItem, DisconnectedEto>
{
    public override partial DisconnectedEto Map(ConnectionPoolCacheItem source);

    public override partial void Map(ConnectionPoolCacheItem source, DisconnectedEto destination);

    public override partial ConnectionPoolCacheItem ReverseMap(DisconnectedEto source);

    public override partial void ReverseMap(DisconnectedEto source, ConnectionPoolCacheItem destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainConnectionPoolCacheItemToConnectedEtoMapper : TwoWayMapperBase<ConnectionPoolCacheItem, ConnectedEto>
{
    public override partial ConnectedEto Map(ConnectionPoolCacheItem source);

    public override partial void Map(ConnectionPoolCacheItem source, ConnectedEto destination);

    public override partial ConnectionPoolCacheItem ReverseMap(ConnectedEto source);

    public override partial void ReverseMap(ConnectedEto source, ConnectionPoolCacheItem destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainConnectionPoolCacheItemToActivedEtoMapper : TwoWayMapperBase<ConnectionPoolCacheItem, ActivedEto>
{
    public override partial ActivedEto Map(ConnectionPoolCacheItem source);

    public override partial void Map(ConnectionPoolCacheItem source, ActivedEto destination);

    public override partial ConnectionPoolCacheItem ReverseMap(ActivedEto source);

    public override partial void ReverseMap(ActivedEto source, ConnectionPoolCacheItem destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainDisconnectedEtoToConnectedEtoMapper : TwoWayMapperBase<DisconnectedEto, ConnectedEto>
{
    public override partial ConnectedEto Map(DisconnectedEto source);

    public override partial void Map(DisconnectedEto source, ConnectedEto destination);

    public override partial DisconnectedEto ReverseMap(ConnectedEto source);

    public override partial void ReverseMap(ConnectedEto source, DisconnectedEto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainDisconnectedEtoToActivedEtoMapper : TwoWayMapperBase<DisconnectedEto, ActivedEto>
{
    public override partial ActivedEto Map(DisconnectedEto source);

    public override partial void Map(DisconnectedEto source, ActivedEto destination);

    public override partial DisconnectedEto ReverseMap(ActivedEto source);

    public override partial void ReverseMap(ActivedEto source, DisconnectedEto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainConnectedEtoToActivedEtoMapper : TwoWayMapperBase<ConnectedEto, ActivedEto>
{
    public override partial ActivedEto Map(ConnectedEto source);

    public override partial void Map(ConnectedEto source, ActivedEto destination);

    public override partial ConnectedEto ReverseMap(ActivedEto source);

    public override partial void ReverseMap(ActivedEto source, ConnectedEto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainSessionUnitSettingToSessionUnitSettingCacheItemMapper : MapperBase<SessionUnitSetting, SessionUnitSettingCacheItem>
{
    public override partial SessionUnitSettingCacheItem Map(SessionUnitSetting source);

    public override partial void Map(SessionUnitSetting source, SessionUnitSettingCacheItem destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ChatDomainBoxToBoxInfoMapper : MapperBase<Box, BoxInfo>
{
    public override partial BoxInfo Map(Box source);

    public override partial void Map(Box source, BoxInfo destination);
}
