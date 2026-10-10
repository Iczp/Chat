using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using Riok.Mapperly.Abstractions;
using System;
using System.Reflection;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

// AutoMapper currently writes these protected setters. Keep that behavior until
// the ChatObject create/update use case is changed explicitly in Domain.
internal static class ChatObjectInputProtectedSetters
{
    private static readonly PropertyInfo Name = GetProperty(nameof(ChatObject.Name));
    private static readonly PropertyInfo VerificationMethod = GetProperty(nameof(ChatObject.VerificationMethod));
    private static readonly PropertyInfo ChatObjectTypeId = GetProperty(nameof(ChatObject.ChatObjectTypeId));
    private static readonly PropertyInfo ObjectType = GetProperty(nameof(ChatObject.ObjectType));

    private static PropertyInfo GetProperty(string name) =>
        typeof(ChatObject).GetProperty(name, BindingFlags.Instance | BindingFlags.Public)
        ?? throw new MissingMemberException(typeof(ChatObject).FullName, name);

    public static ChatObject Create() =>
        (ChatObject)Activator.CreateInstance(typeof(ChatObject), nonPublic: true)!;

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
public partial class ChatApplicationChatObjectCreateInputToChatObjectMapper
    : MapperBase<ChatObjectCreateInput, ChatObject>
{
    public override ChatObject Map(ChatObjectCreateInput source)
    {
        if (source is null) return default!;
        var destination = ChatObjectInputProtectedSetters.Create();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObjectCreateInput source, ChatObject destination)
    {
        if (source is null) return;
        MapPublicProperties(source, destination);
        ChatObjectInputProtectedSetters.Apply(source, destination);
    }

    private partial void MapPublicProperties(ChatObjectCreateInput source, ChatObject destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationChatObjectUpdateInputToChatObjectMapper
    : MapperBase<ChatObjectUpdateInput, ChatObject>
{
    public override ChatObject Map(ChatObjectUpdateInput source)
    {
        if (source is null) return default!;
        var destination = ChatObjectInputProtectedSetters.Create();
        Map(source, destination);
        return destination;
    }

    public override void Map(ChatObjectUpdateInput source, ChatObject destination)
    {
        if (source is null) return;
        MapPublicProperties(source, destination);
        ChatObjectInputProtectedSetters.Apply(source, destination);
    }

    private partial void MapPublicProperties(ChatObjectUpdateInput source, ChatObject destination);
}
