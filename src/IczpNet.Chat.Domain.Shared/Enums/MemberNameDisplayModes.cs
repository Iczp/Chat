using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    /// <summary>
    /// 群类型,
    /// 0:自由群（常规群）;
    /// 1:Condition条件群(系统群 System);
    /// 2:Tasks任务群;
    /// 3:Course课程群;
    /// </summary>
    [Description("群类型")]
    public enum MemberNameDisplayModes 
    {
        /// <summary>
        /// 自定义
        /// </summary>
        [Description("自定义")]
        Default = 0,
        /// <summary>
        /// 系统角色
        /// </summary>
        [Description("系统角色")]
        SystemRole = 1,
        /// <summary>
        /// 群角色
        /// </summary>
        [Description("群角色")]
        RoomRole = 2,
        /// <summary>
        /// 用户身份
        /// </summary>
        [Description("用户身份")]
        Position = 3
    }
}
