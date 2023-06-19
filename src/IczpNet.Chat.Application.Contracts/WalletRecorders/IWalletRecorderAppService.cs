using IczpNet.Chat.WalletRecorders.Dtos;
using System;

namespace IczpNet.Chat.WalletRecorders
{
    public interface IWalletRecorderAppService :
        ICrudChatAppService<
            WalletRecorderDto,
            WalletRecorderDto,
            Guid,
            WalletRecorderGetListInput>
    {

    }
}
