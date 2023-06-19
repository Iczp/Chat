using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.WalletOrders
{
    public class WalletOrderNoGenerator : DomainService, IWalletOrderNoGenerator
    {
        public WalletOrderNoGenerator()
        {
        }

        public async Task<string> MakeAsync()
        {
            await Task.Yield();
            return Clock.Now.ToString("yyyyMMddHHmmssffffff");
        }
    }
}
