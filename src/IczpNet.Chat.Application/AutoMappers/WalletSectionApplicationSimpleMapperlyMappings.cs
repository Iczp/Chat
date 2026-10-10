// Generated from conventional CreateMap declarations. Keep complex maps in the profile until migrated.
using IczpNet.Chat.Wallets;
using IczpNet.Chat.Wallets.Dtos;
using IczpNet.Chat.WalletOrders;
using IczpNet.Chat.WalletOrders.Dtos;
using IczpNet.Chat.WalletRecorders.Dtos;
using IczpNet.Chat.WalletRecorders;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class WalletSectionApplicationWalletToWalletDtoMapper : MapperBase<Wallet, WalletDto>
{
    public override partial WalletDto Map(Wallet source);

    public override partial void Map(Wallet source, WalletDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class WalletSectionApplicationWalletToWalletDetailDtoMapper : MapperBase<Wallet, WalletDetailDto>
{
    public override partial WalletDetailDto Map(Wallet source);

    public override partial void Map(Wallet source, WalletDetailDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class WalletSectionApplicationWalletRecorderToWalletRecorderDtoMapper : MapperBase<WalletRecorder, WalletRecorderDto>
{
    public override partial WalletRecorderDto Map(WalletRecorder source);

    public override partial void Map(WalletRecorder source, WalletRecorderDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class WalletSectionApplicationWalletOrderToWalletOrderDtoMapper : MapperBase<WalletOrder, WalletOrderDto>
{
    public override partial WalletOrderDto Map(WalletOrder source);

    public override partial void Map(WalletOrder source, WalletOrderDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class WalletSectionApplicationWalletOrderToWalletOrderDetailDtoMapper : MapperBase<WalletOrder, WalletOrderDetailDto>
{
    public override partial WalletOrderDetailDto Map(WalletOrder source);

    public override partial void Map(WalletOrder source, WalletOrderDetailDto destination);
}
