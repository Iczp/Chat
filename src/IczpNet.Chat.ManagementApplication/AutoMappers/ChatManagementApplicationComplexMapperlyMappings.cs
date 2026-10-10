using IczpNet.Chat.Articles;
using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.ChatObjectCategories;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Connections;
using IczpNet.Chat.Management.Articles.Dtos;
using IczpNet.Chat.Management.ChatObjectCategorys.Dtos;
using IczpNet.Chat.Management.ChatObjects.Dtos;
using IczpNet.Chat.Management.ChatObjectTypes.Dtos;
using IczpNet.Chat.Management.Connections.Dtos;
using IczpNet.Chat.Management.Mottos.Dtos;
using IczpNet.Chat.Mottos;
using Riok.Mapperly.Abstractions;
using System.Reflection;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.Management.AutoMappers;

internal static class ChatObjectInputProtectedSetters
{
    private static readonly PropertyInfo Name = typeof(ChatObject).GetProperty(nameof(ChatObject.Name), BindingFlags.Instance | BindingFlags.Public)!;
    private static readonly PropertyInfo VerificationMethod = typeof(ChatObject).GetProperty(nameof(ChatObject.VerificationMethod), BindingFlags.Instance | BindingFlags.Public)!;
    private static readonly PropertyInfo ChatObjectTypeId = typeof(ChatObject).GetProperty(nameof(ChatObject.ChatObjectTypeId), BindingFlags.Instance | BindingFlags.Public)!;
    private static readonly PropertyInfo ObjectType = typeof(ChatObject).GetProperty(nameof(ChatObject.ObjectType), BindingFlags.Instance | BindingFlags.Public)!;

    public static void Apply(ChatObjectUpdateInput source, ChatObject destination)
    {
        Name.SetValue(destination, source.Name);
        VerificationMethod.SetValue(destination, source.VerificationMethod);
    }

    public static void Apply(ChatObjectCreateInput source, ChatObject destination)
    {
        Apply((ChatObjectUpdateInput)source, destination);
        ChatObjectTypeId.SetValue(destination, source.ChatObjectTypeId);
        ObjectType.SetValue(destination, source.ObjectType);
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatManagementApplicationChatObjectToChatObjectDetailDtoMapper
    : MapperBase<ChatObject, ChatObjectDetailDto>
{
    public override ChatObjectDetailDto Map(ChatObject source)
    {
        if (source is null) return default!;
        var destination = new ChatObjectDetailDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObject source, ChatObjectDetailDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.SenderMessageCount = source.SenderMessageList?.Count ?? 0;
        destination.ReceiverMessageCount = source.ReceiverMessageList?.Count ?? 0;
        destination.FriendCount = source.OwnerFriendshipList?.Count ?? 0;
        destination.InFriendCount = source.DestinationFriendshipList?.Count ?? 0;
    }

    [MapperIgnoreTarget(nameof(ChatObjectDetailDto.SenderMessageCount))]
    [MapperIgnoreTarget(nameof(ChatObjectDetailDto.ReceiverMessageCount))]
    [MapperIgnoreTarget(nameof(ChatObjectDetailDto.FriendCount))]
    [MapperIgnoreTarget(nameof(ChatObjectDetailDto.InFriendCount))]
    private partial void MapFields(ChatObject source, ChatObjectDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatManagementApplicationChatObjectCreateInputToChatObjectMapper
    : MapperBase<ChatObjectCreateInput, ChatObject>
{
    public override ChatObject Map(ChatObjectCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<ChatObject>();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObjectCreateInput source, ChatObject destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
        ChatObjectInputProtectedSetters.Apply(source, destination);
    }

    private partial void MapPublicProperties(ChatObjectCreateInput source, ChatObject destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatManagementApplicationChatObjectUpdateInputToChatObjectMapper
    : MapperBase<ChatObjectUpdateInput, ChatObject>
{
    public override ChatObject Map(ChatObjectUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<ChatObject>();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObjectUpdateInput source, ChatObject destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
        ChatObjectInputProtectedSetters.Apply(source, destination);
    }

    private partial void MapPublicProperties(ChatObjectUpdateInput source, ChatObject destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatManagementApplicationMottoCreateInputToMottoMapper
    : MapperBase<MottoCreateInput, Motto>
{
    public override Motto Map(MottoCreateInput source)
    {
        if (source is null) return default!;
        var destination = new Motto();
        Map(source, destination);
        return destination;
    }

    public override void Map(MottoCreateInput source, Motto destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(MottoCreateInput source, Motto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatManagementApplicationMottoUpdateInputToMottoMapper
    : MapperBase<MottoUpdateInput, Motto>
{
    public override Motto Map(MottoUpdateInput source)
    {
        if (source is null) return default!;
        var destination = new Motto();
        Map(source, destination);
        return destination;
    }

    public override void Map(MottoUpdateInput source, Motto destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(MottoUpdateInput source, Motto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatManagementApplicationChatObjectCategoryCreateInputToChatObjectCategoryMapper
    : MapperBase<ChatObjectCategoryCreateInput, ChatObjectCategory>
{
    public override ChatObjectCategory Map(ChatObjectCategoryCreateInput source)
    {
        if (source is null) return default!;
        var destination = new ChatObjectCategory();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObjectCategoryCreateInput source, ChatObjectCategory destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ChatObjectCategoryCreateInput source, ChatObjectCategory destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatManagementApplicationChatObjectCategoryUpdateInputToChatObjectCategoryMapper
    : MapperBase<ChatObjectCategoryUpdateInput, ChatObjectCategory>
{
    public override ChatObjectCategory Map(ChatObjectCategoryUpdateInput source)
    {
        if (source is null) return default!;
        var destination = new ChatObjectCategory();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObjectCategoryUpdateInput source, ChatObjectCategory destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ChatObjectCategoryUpdateInput source, ChatObjectCategory destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatManagementApplicationChatObjectTypeToChatObjectTypeDtoMapper
    : MapperBase<ChatObjectType, ChatObjectTypeDto>
{
    public override ChatObjectTypeDto Map(ChatObjectType source)
    {
        if (source is null) return default!;
        var destination = new ChatObjectTypeDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObjectType source, ChatObjectTypeDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.ChatObjectCount = source.GetChatObjectCount();
    }

    [MapperIgnoreTarget(nameof(ChatObjectTypeDto.ChatObjectCount))]
    private partial void MapFields(ChatObjectType source, ChatObjectTypeDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatManagementApplicationChatObjectTypeToChatObjectTypeDetailDtoMapper
    : MapperBase<ChatObjectType, ChatObjectTypeDetailDto>
{
    public override ChatObjectTypeDetailDto Map(ChatObjectType source)
    {
        if (source is null) return default!;
        var destination = new ChatObjectTypeDetailDto();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObjectType source, ChatObjectTypeDetailDto destination)
    {
        if (source is null || destination is null) return;
        MapFields(source, destination);
        destination.ChatObjectCount = source.GetChatObjectCount();
    }

    [MapperIgnoreTarget(nameof(ChatObjectTypeDetailDto.ChatObjectCount))]
    private partial void MapFields(ChatObjectType source, ChatObjectTypeDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatManagementApplicationChatObjectTypeCreateInputToChatObjectTypeMapper
    : MapperBase<ChatObjectTypeCreateInput, ChatObjectType>
{
    public override ChatObjectType Map(ChatObjectTypeCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<ChatObjectType>();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObjectTypeCreateInput source, ChatObjectType destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ChatObjectTypeCreateInput source, ChatObjectType destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatManagementApplicationChatObjectTypeUpdateInputToChatObjectTypeMapper
    : MapperBase<ChatObjectTypeUpdateInput, ChatObjectType>
{
    public override ChatObjectType Map(ChatObjectTypeUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<ChatObjectType>();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObjectTypeUpdateInput source, ChatObjectType destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ChatObjectTypeUpdateInput source, ChatObjectType destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatManagementApplicationArticleCreateInputToArticleMapper
    : MapperBase<ArticleCreateInput, Article>
{
    public override Article Map(ArticleCreateInput source)
    {
        if (source is null) return default!;
        var destination = new Article();
        Map(source, destination);
        return destination;
    }

    public override void Map(ArticleCreateInput source, Article destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ArticleCreateInput source, Article destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatManagementApplicationArticleUpdateInputToArticleMapper
    : MapperBase<ArticleUpdateInput, Article>
{
    public override Article Map(ArticleUpdateInput source)
    {
        if (source is null) return default!;
        var destination = new Article();
        Map(source, destination);
        return destination;
    }

    public override void Map(ArticleUpdateInput source, Article destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ArticleUpdateInput source, Article destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatManagementApplicationConnectionCreateInputToConnectionMapper
    : MapperBase<ConnectionCreateInput, Connection>
{
    public override Connection Map(ConnectionCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<Connection>();
        Map(source, destination);
        return destination;
    }

    public override void Map(ConnectionCreateInput source, Connection destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(ConnectionCreateInput source, Connection destination);
}
