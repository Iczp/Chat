using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    [Description("钱包业务类型")]
    public enum WalletBusinessTypes : int
    {
        /// <summary>
        /// 收入
        /// </summary>
        [Description("收入")]
        Income = 1,
        /// <summary>
        /// 支出
        /// </summary>
        [Description("支出")]
        Expenditure = -1,
    }
}
