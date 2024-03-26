namespace IczpNet.Chat.Options;

public class ChatOption
{
    /// <summary>
    /// 超过{HOURS}小时的消息不能被撤回,默认 24H
    /// </summary>
    public int AllowRollbackHours { get; set; } = 24;

    ///// <summary>
    ///// 会话最大关注数量
    ///// </summary>
    //public int MaxFollowingCount { get; set; } = 10;

    public PortraitOptions PortraitSetting { get; set; } = new();

    public class PortraitOptions
    {
        /// <summary>
        /// 略缩图大小
        /// </summary>
        public int PortraitThumbnailSize { get; set; } = 128;

        /// <summary>
        /// 大图大小
        /// </summary>
        public int PortraitBigSize { get; set; } = 540;

    }
}
