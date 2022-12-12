using System.ComponentModel;

namespace IczpNet.Chat.Enums
{

    public enum WalletBusinessTypes : int
    {
        /// <summary>
        /// 收入
        /// </summary>
        [Description("收入")]
        Income = 0,
        /// <summary>
        /// 支出
        /// </summary>
        [Description("支出")]
        Expenditure = 1,
    }
}
