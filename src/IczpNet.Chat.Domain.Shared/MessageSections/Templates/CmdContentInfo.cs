namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 系统命令（）
    /// </summary>
    //[AutoMap(typeof(CmdContent))]
    public class CmdContentInfo : BaseMessageContentInfo, IMessageContentInfo
    {
        /// <summary>
        /// 执行的命令（需要与前端一致）
        /// </summary>
        public virtual string Cmd { get; set; }
        /// <summary>
        /// 显示内容
        /// </summary>

        public virtual string Text { get; set; }
        /// <summary>
        /// app:///pages/im/notice?id=123
        /// </summary>
        public virtual string Url { get; set; }

    }
}