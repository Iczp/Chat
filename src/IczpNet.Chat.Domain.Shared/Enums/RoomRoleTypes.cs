using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    /// <summary>
    /// 群角色(0:群成员,1:群管理员,2:群主)
    /// </summary>
    public enum RoomRoleTypes
    {
        /// <summary>
        /// 群成员
        /// </summary>
        [Description("群成员")]
        Member = 0,
        /// <summary>
        /// 群管理员
        /// </summary>
        [Description("群管理员")]
        Manager = 1,
        /// <summary>
        /// 群主
        /// </summary>
        [Description("群主")]
        Creator = 2,
    }
}
