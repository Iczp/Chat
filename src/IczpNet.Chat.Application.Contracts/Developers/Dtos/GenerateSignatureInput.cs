namespace IczpNet.Chat.Developers.Dtos
{
    /// <summary>
    /// 生成签名
    /// </summary>
    public class GenerateSignatureInput
    {
        /// <summary>
        /// 公众平台上，开发者设置的 Token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 随机串，对应URL参数的 nonce
        /// </summary>
        public string Nonce { get; set; }
        /// <summary>
        /// 时间戳，对应URL参数的 timestamp
        /// </summary>
        public string TimeStamp { get; set; }
        /// <summary>
        /// 随机串，对应URL参数的 Echo
        /// </summary>
        public string Echo { get; set; }
    }
}
