using AutoMapper;
using IczpNet.Chat.Wallets;
using IczpNet.Chat.Wallets.Dtos;

namespace IczpNet.Chat.AutoMappers;

public class WalletSectionApplicationAutoMapperProfile : Profile
{
    public WalletSectionApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        //Wallet
        CreateMap<Wallet, WalletDto>();
        CreateMap<Wallet, WalletDetailDto>();
        //CreateMap<WalletCreateInput, Wallet>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
        //CreateMap<WalletUpdateInput, Wallet>(MemberList.Source).IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
    }
}
