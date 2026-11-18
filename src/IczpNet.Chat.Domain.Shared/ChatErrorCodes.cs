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

    public static class Blobs
    {
        public const string FileEmpty = "Chat:Blobs:0001";
        public const string FileSizeExceeded = "Chat:Blobs:0002";
        public const string FileTypeNotAllowed = "Chat:Blobs:0003";
        public const string FileHeaderMismatch = "Chat:Blobs:0004";
        public const string InvalidMediaMetadata = "Chat:Blobs:0005";
        public const string AttachmentNotFoundOrForbidden = "Chat:Blobs:0006";
        public const string AttachmentAlreadyBound = "Chat:Blobs:0007";
        public const string AccessDenied = "Chat:Blobs:0008";
        public const string InvalidSignature = "Chat:Blobs:0009";
    }
}
