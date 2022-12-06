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
    public enum RoomTypes 
    {
        /// <summary>
        /// 自由群（用户群 User）
        /// </summary>
        [Description("自由群（常规群）")]
        Normal = 0,
        /// <summary>
        /// 条件群(系统群 System)
        /// </summary>
        [Description("条件群")]
        Condition = 1,
        /// <summary>
        /// 任务群
        /// </summary>
        [Description("任务群")]
        Tasks = 2,
        /// <summary>
        /// 课程群
        /// </summary>
        [Description("课程群")]
        Course = 3,
        /// <summary>
        /// 工程项目群
        /// </summary>
        [Description("工程项目群")]
        EngineeringProject = 4,
        /// <summary>
        /// 工程标准群
        /// </summary>
        [Description("工程标准群")]
        EngineeringStandard = 5,
    }
}
