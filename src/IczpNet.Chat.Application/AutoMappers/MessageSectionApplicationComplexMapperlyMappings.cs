using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Riok.Mapperly.Abstractions;
using System.Collections.Generic;
using Volo.Abp.Data;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

internal static class ExtraPropertiesMapperHelper
{
    public static void CopyExtraProperties(IHasExtraProperties source, IHasExtraProperties destination)
    {
        if (source?.ExtraProperties == null || destination?.ExtraProperties == null) return;
        foreach (KeyValuePair<string, object> item in source.ExtraProperties)
        {
            destination.SetProperty(item.Key, item.Value);
        }
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationMessageToMessageDtoMapper
    : MapperBase<Message, MessageDto>
{
    public override MessageDto Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Content = source.GetContentDto();
        ExtraPropertiesMapperHelper.CopyExtraProperties(source, destination);
    }

    [MapperIgnoreTarget(nameof(MessageDto.Content))]
    [MapperIgnoreTarget(nameof(MessageDto.ExtraProperties))]
    private partial void MapFields(Message source, MessageDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationMessageToMessageOwnerDtoMapper
    : MapperBase<Message, MessageOwnerDto>
{
    public override MessageOwnerDto Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageOwnerDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageOwnerDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Content = source.GetContentDto();
        ExtraPropertiesMapperHelper.CopyExtraProperties(source, destination);
    }

    [MapperIgnoreTarget(nameof(MessageOwnerDto.Content))]
    [MapperIgnoreTarget(nameof(MessageOwnerDto.ExtraProperties))]
    private partial void MapFields(Message source, MessageOwnerDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationMessageToMessageDetailDtoMapper
    : MapperBase<Message, MessageDetailDto>
{
    public override MessageDetailDto Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageDetailDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageDetailDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        ExtraPropertiesMapperHelper.CopyExtraProperties(source, destination);
    }

    [MapperIgnoreTarget(nameof(MessageDetailDto.ExtraProperties))]
    private partial void MapFields(Message source, MessageDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationMessageToMessageFavoriteDtoMapper
    : MapperBase<Message, MessageFavoriteDto>
{
    public override MessageFavoriteDto Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageFavoriteDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageFavoriteDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Content = source.GetContentDto();
    }

    [MapperIgnoreTarget(nameof(MessageFavoriteDto.Content))]
    private partial void MapFields(Message source, MessageFavoriteDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationMessageToMessageQuoteDtoMapper
    : MapperBase<Message, MessageQuoteDto>
{
    public override MessageQuoteDto Map(Message source)
    {
        if (source is null) return default!;
        var destination = new MessageQuoteDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(Message source, MessageQuoteDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        ExtraPropertiesMapperHelper.CopyExtraProperties(source, destination);
    }

    [MapperIgnoreTarget(nameof(MessageQuoteDto.ExtraProperties))]
    private partial void MapFields(Message source, MessageQuoteDto destination);
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

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationMessageCacheItemToMessageFastDtoMapper
    : MapperBase<MessageCacheItem, MessageFastDto>
{
    public override MessageFastDto Map(MessageCacheItem source)
    {
        if (source is null) return default!;
        var destination = new MessageFastDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(MessageCacheItem source, MessageFastDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        ExtraPropertiesMapperHelper.CopyExtraProperties(source, destination);
    }

    [MapperIgnoreTarget(nameof(MessageFastDto.ExtraProperties))]
    private partial void MapFields(MessageCacheItem source, MessageFastDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationMessageCacheItemToMessageOwnerDtoMapper
    : MapperBase<MessageCacheItem, MessageOwnerDto>
{
    public override MessageOwnerDto Map(MessageCacheItem source)
    {
        if (source is null) return default!;
        var destination = new MessageOwnerDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(MessageCacheItem source, MessageOwnerDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        ExtraPropertiesMapperHelper.CopyExtraProperties(source, destination);
    }

    [MapperIgnoreTarget(nameof(MessageOwnerDto.ExtraProperties))]
    private partial void MapFields(MessageCacheItem source, MessageOwnerDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationMessageQuoteCacheItemToMessageQuoteDtoMapper
    : MapperBase<MessageQuoteCacheItem, MessageQuoteDto>
{
    public override MessageQuoteDto Map(MessageQuoteCacheItem source)
    {
        if (source is null) return default!;
        var destination = new MessageQuoteDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(MessageQuoteCacheItem source, MessageQuoteDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        ExtraPropertiesMapperHelper.CopyExtraProperties(source, destination);
    }

    [MapperIgnoreTarget(nameof(MessageQuoteDto.ExtraProperties))]
    private partial void MapFields(MessageQuoteCacheItem source, MessageQuoteDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationMessageQuoteCacheItemToMessageQuoteFastDtoMapper
    : MapperBase<MessageQuoteCacheItem, MessageQuoteFastDto>
{
    public override MessageQuoteFastDto Map(MessageQuoteCacheItem source)
    {
        if (source is null) return default!;
        var destination = new MessageQuoteFastDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(MessageQuoteCacheItem source, MessageQuoteFastDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        ExtraPropertiesMapperHelper.CopyExtraProperties(source, destination);
    }

    [MapperIgnoreTarget(nameof(MessageQuoteFastDto.ExtraProperties))]
    private partial void MapFields(MessageQuoteCacheItem source, MessageQuoteFastDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationSessionUnitSenderInfoToSessionUnitSenderDtoMapper
    : MapperBase<SessionUnitSenderInfo, SessionUnitSenderDto>
{
    public override SessionUnitSenderDto Map(SessionUnitSenderInfo source)
    {
        if (source is null) return default!;
        var destination = new SessionUnitSenderDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionUnitSenderInfo source, SessionUnitSenderDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Score = MemberScore.Create(source.IsCreator, source.CreationTime);
    }

    [MapperIgnoreTarget(nameof(SessionUnitSenderDto.Score))]
    private partial void MapFields(SessionUnitSenderInfo source, SessionUnitSenderDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class MessageSectionApplicationSessionUnitCacheItemToSessionUnitSenderDtoMapper
    : MapperBase<SessionUnitCacheItem, SessionUnitSenderDto>
{
    public override SessionUnitSenderDto Map(SessionUnitCacheItem source)
    {
        if (source is null) return default!;
        var destination = new SessionUnitSenderDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(SessionUnitCacheItem source, SessionUnitSenderDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.Score = MemberScore.Create(source.IsCreator, source.CreationTime);
    }

    [MapperIgnoreTarget(nameof(SessionUnitSenderDto.Score))]
    private partial void MapFields(SessionUnitCacheItem source, SessionUnitSenderDto destination);
}
