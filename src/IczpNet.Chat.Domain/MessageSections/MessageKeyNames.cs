

namespace IczpNet.Chat.MessageSections
{
    /// <summary>
    /// KeyNameConsts
    /// </summary>
    public static class MessageKeyNames
    {
       

        /// <summary>
        /// CreateRoom  创建群
        /// </summary>
        public static string CreateRoom { get { return "CreateRoom"; } }
        /// <summary>
        /// CreateConditionRoom  创建条件群
        /// </summary>
        public static string CreateConditionRoom { get { return "CreateConditionRoom"; } }
        /// <summary>
        /// UpdateRoomName  修改群名称
        /// </summary>
        public static string UpdateRoomName { get { return "UpdateRoomName"; } }
        /// <summary>
        /// UpdatePortrait  更新群头像
        /// </summary>
        public static string UpdatePortrait { get { return "UpdatePortrait"; } }
        /// <summary>
        /// RemoveRoomMembers  移除成员
        /// </summary>
        public static string RemoveRoomMembers { get { return "RemoveRoomMembers"; } }
        /// <summary>
        /// InviteRoomMember  邀请成员加群
        /// </summary>
        public static string InviteRoomMember { get { return "InviteRoomMember"; } }
        /// <summary>
        /// ForbiddenMember  禁言群成员
        /// </summary>
        public static string ForbiddenMember { get { return "ForbiddenMember"; } }
        /// <summary>
        /// UnsubscribeUser  取消关注（移出管理群）
        /// </summary>
        public static string UnsubscribeUser{ get { return "UnsubscribeUser"; } }
        /// <summary>
        /// SubscribeUser  添加关注（加入管理群）
        /// </summary>
        public static string SubscribeUser{ get { return "SubscribeUser"; } }
        /// <summary>
        /// Slapped  拍一拍
        /// </summary>
        public static string Slapped { get { return "Slapped"; } }
        /// <summary>
        /// Remind  @Ta 提醒功能 @XXX
        /// </summary>
        public static string Remind { get { return "Remind"; } }
        /// <summary>
        /// EVERYONE  @所有人
        /// </summary>
        public static string RemindEveryone { get { return "EVERYONE"; } }
        /// <summary>
        /// 变更任务状态
        /// </summary>
        public static string ChangedProcessState { get { return "ChangedProcessState"; } }

        /// <summary>
        /// 位置共享(ShareLocation)
        /// </summary>
        public static string ShareLocation { get { return "ShareLocation"; } }
        /// <summary>
        /// 发起位置共享
        /// </summary>
        public static string CreateShareLocation { get { return "CreateShareLocation"; } }
        /// <summary>
        /// 加入位置共享
        /// </summary>
        public static string JoinShareLocation { get { return "JoinShareLocation"; } }
        /// <summary>
        /// 更新位置信息
        /// </summary>
        public static string UpdateShareLocation { get { return "UpdateShareLocation"; } }
        /// <summary>
        /// 退出位置共享
        /// </summary>
        public static string StopShareLocation { get { return "StopShareLocation"; } }
    }

}
