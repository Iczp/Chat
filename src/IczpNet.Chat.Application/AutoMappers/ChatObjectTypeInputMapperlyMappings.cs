using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.ChatObjectTypes.Dtos;
using Riok.Mapperly.Abstractions;
using System;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationChatObjectTypeCreateInputToChatObjectTypeMapper
    : MapperBase<ChatObjectTypeCreateInput, ChatObjectType>
{
    public override ChatObjectType Map(ChatObjectTypeCreateInput source)
    {
        if (source is null) return default!;
        var destination = new ChatObjectType(source.Id);
        Map(source, destination);
        return destination;
    }

    public override partial void Map(ChatObjectTypeCreateInput source, ChatObjectType destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationChatObjectTypeUpdateInputToChatObjectTypeMapper
    : MapperBase<ChatObjectTypeUpdateInput, ChatObjectType>
{
    public override ChatObjectType Map(ChatObjectTypeUpdateInput source)
    {
        if (source is null) return default!;
        var destination = (ChatObjectType)Activator.CreateInstance(typeof(ChatObjectType), nonPublic: true)!;
        Map(source, destination);
        return destination;
    }

    public override partial void Map(ChatObjectTypeUpdateInput source, ChatObjectType destination);
}
