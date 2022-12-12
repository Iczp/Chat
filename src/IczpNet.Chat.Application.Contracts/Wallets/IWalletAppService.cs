using IczpNet.Chat.Wallets.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Wallets;

public interface IWalletAppService :
    ICrudAppService<
        WalletDetailDto,
        WalletDto,
        Guid,
        WalletGetListInput,
        WalletCreateInput,
        WalletUpdateInput>
{

    public Task<WalletDto> Recharge(RechargeInput input);
}
