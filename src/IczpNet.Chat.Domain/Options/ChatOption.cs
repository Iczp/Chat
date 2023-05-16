namespace IczpNet.Chat.Options
{
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
    }
}
