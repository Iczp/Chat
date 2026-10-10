using IczpNet.Chat.WalletOrders;
using IczpNet.Chat.WalletOrders.Dtos;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class WalletSectionApplicationWalletOrderCreateInputToWalletOrderMapper
    : MapperBase<WalletOrderCreateInput, WalletOrder>
{
    public override WalletOrder Map(WalletOrderCreateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<WalletOrder>();
        Map(source, destination);
        return destination;
    }

    public override void Map(WalletOrderCreateInput source, WalletOrder destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(WalletOrderCreateInput source, WalletOrder destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class WalletSectionApplicationWalletOrderUpdateInputToWalletOrderMapper
    : MapperBase<WalletOrderUpdateInput, WalletOrder>
{
    public override WalletOrder Map(WalletOrderUpdateInput source)
    {
        if (source is null) return default!;
        var destination = EntityActivator.Create<WalletOrder>();
        Map(source, destination);
        return destination;
    }

    public override void Map(WalletOrderUpdateInput source, WalletOrder destination)
    {
        if (source is null || destination is null) return;
        MapPublicProperties(source, destination);
    }

    private partial void MapPublicProperties(WalletOrderUpdateInput source, WalletOrder destination);
}
