using IczpNet.Chat.Management.Wallets.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Management.Wallets;

public interface IWalletManagementAppService :
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
