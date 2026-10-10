// Generated from conventional CreateMap declarations. Keep complex maps in the profile until migrated.
using IczpNet.Chat.Management.Wallets.Dtos;
using IczpNet.Chat.Wallets;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.Management.AutoMappers;

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
