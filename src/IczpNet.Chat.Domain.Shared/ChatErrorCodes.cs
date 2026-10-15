namespace IczpNet.Chat;

public static class ChatErrorCodes
{
    //Add your business exception error codes here...
    public static class Security
    {
        /// <summary>
        /// 消息中包含被平台安全中心拦截的违规外部链接
        /// </summary>
        public const string MessageContainsProhibitedUrl = "Chat:Security:0001";
    }
}
