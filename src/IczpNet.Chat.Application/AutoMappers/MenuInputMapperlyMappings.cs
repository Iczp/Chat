using IczpNet.Chat.Menus;
using IczpNet.Chat.Menus.Dtos;
using Riok.Mapperly.Abstractions;
using System;
using System.Reflection;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

internal static class MenuInputProtectedSetters
{
    private static readonly PropertyInfo Name = typeof(Menu).GetProperty(nameof(Menu.Name))
        ?? throw new MissingMemberException(typeof(Menu).FullName, nameof(Menu.Name));

    public static void Apply(MenuUpdateInput source, Menu destination) =>
        Name.SetValue(destination, source.Name);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationMenuCreateInputToMenuMapper : MapperBase<MenuCreateInput, Menu>
{
    public override Menu Map(MenuCreateInput source)
    {
        if (source is null) return default!;
        var destination = new Menu();
        Map(source, destination);
        return destination;
    }

    public override void Map(MenuCreateInput source, Menu destination)
    {
        if (source is null) return;
        MapPublicProperties(source, destination);
        MenuInputProtectedSetters.Apply(source, destination);
    }

    private partial void MapPublicProperties(MenuCreateInput source, Menu destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatApplicationMenuUpdateInputToMenuMapper : MapperBase<MenuUpdateInput, Menu>
{
    public override Menu Map(MenuUpdateInput source)
    {
        if (source is null) return default!;
        var destination = new Menu();
        Map(source, destination);
        return destination;
    }

    public override void Map(MenuUpdateInput source, Menu destination)
    {
        if (source is null) return;
        MapPublicProperties(source, destination);
        MenuInputProtectedSetters.Apply(source, destination);
    }

    private partial void MapPublicProperties(MenuUpdateInput source, Menu destination);
}
