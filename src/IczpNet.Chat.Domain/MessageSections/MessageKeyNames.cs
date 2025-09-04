

namespace IczpNet.Chat.MessageSections;

/// <summary>
/// KeyNameConsts
/// </summary>
public static class MessageKeyNames
{
    

    /// <summary>
    /// CreateRoom  创建群
    /// </summary>
    public static string CreateRoom { get; set; } = nameof(CreateRoom);
    /// <summary>
    /// 
    /// </summary>
    public static string JoinRoom { get; set; } = nameof(JoinRoom);
    /// <summary>
    /// CreateConditionRoom  创建条件群
    /// </summary>
    public static string CreateConditionRoom { get; set; } = nameof(CreateConditionRoom);
    /// <summary>
    /// UpdateRoomName  修改群名称
    /// </summary>
    public static string UpdateRoomName { get; set; } = nameof(UpdateRoomName);
    /// <summary>
    /// UpdatePortrait  更新群头像
    /// </summary>
    public static string UpdatePortrait { get; set; } = nameof(UpdatePortrait);
    /// <summary>
    /// RemoveRoomMembers  移除成员
    /// </summary>
    public static string RemoveRoomMembers { get; set; } = nameof(RemoveRoomMembers);
    /// <summary>
    /// InviteRoomMember  邀请成员加群
    /// </summary>
    public static string InviteRoomMember { get; set; } = nameof(InviteRoomMember);
    /// <summary>
    /// ForbiddenMember  禁言群成员
    /// </summary>
    public static string ForbiddenMember { get; set; } = nameof(ForbiddenMember);
    /// <summary>
    /// UnsubscribeUser  取消关注（移出管理群）
    /// </summary>
    public static string UnsubscribeUser { get; set; } = nameof(UnsubscribeUser);
    /// <summary>
    /// SubscribeUser  添加关注（加入管理群）
    /// </summary>
    public static string SubscribeUser { get; set; } = nameof(SubscribeUser);
    /// <summary>
    /// Slapped  拍一拍
    /// </summary>
    public static string Slapped { get; set; } = nameof(Slapped);
    /// <summary>
    /// Remind  @Ta 提醒功能 @XXX
    /// </summary>
    public static string Remind { get; set; } = nameof(Remind);
    /// <summary>
    /// EVERYONE  @所有人
    /// </summary>
    public static string RemindEveryone { get; set; } = nameof(RemindEveryone);
    /// <summary>
    /// 变更任务状态
    /// </summary>
    public static string ChangedProcessState { get; set; } = nameof(ChangedProcessState);

    /// <summary>
    /// 位置共享(ShareLocation)
    /// </summary>
    public static string ShareLocation { get; set; } = nameof(ShareLocation);
    /// <summary>
    /// 发起位置共享
    /// </summary>
    public static string CreateShareLocation { get; set; } = nameof(CreateShareLocation);
    /// <summary>
    /// 加入位置共享
    /// </summary>
    public static string JoinShareLocation { get; set; } = nameof(JoinShareLocation);
    /// <summary>
    /// 更新位置信息
    /// </summary>
    public static string UpdateShareLocation { get; set; } = nameof(UpdateShareLocation);
    /// <summary>
    /// 退出位置共享
    /// </summary>
    public static string StopShareLocation { get; set; } = nameof(StopShareLocation);
    /// <summary>
    /// 客服转接
    /// </summary>
    public static string Transfer { get; set; } = nameof(Transfer);
    /// <summary>
    /// 转让群主
    /// </summary>
    public static string TransferCreator { get; set; } = nameof(TransferCreator);
    /// <summary>
    /// 添加好友
    /// </summary>
    public static string AddFriendSuccess { get; set; } = nameof(AddFriendSuccess);
}
