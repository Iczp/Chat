namespace IczpNet.Chat.Developers.Dtos
{
    public class VerifySignatureInput
    {
        /// <summary>
        /// 签名 signature
        /// </summary>
        public virtual string Signature { get; set; }

        /// <summary>
        /// 公众平台上，开发者设置的Token
        /// </summary>
        public virtual string Token { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public virtual string TimeStamp { get; set; }

        /// <summary>
        /// 随机数
        /// </summary>
        public virtual string Nonce { get; set; }

        /// <summary>
        /// 密文
        /// </summary>
        public virtual string CipherText { get; set; }
    }
}
