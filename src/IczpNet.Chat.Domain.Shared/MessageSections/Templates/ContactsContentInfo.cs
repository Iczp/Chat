using IczpNet.Chat.Enums;

namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 联系人消息
    /// </summary>
    //[AutoMap(typeof(ContactsContent))]
    public class ContactsContentInfo : BaseMessageContentInfo, IMessageContentInfo
    {
        /// <summary>
        /// 媒体Id
        /// </summary>
        public string MediaId { get; set; }

        /// <summary>
        /// 媒体类型  0:未定义, 1:个人, 2:群, 3:订阅号, 4:公众号, 5:部门群, 6:课程群, 7:任务群
        /// </summary>
        public MessageTypes MessageType { get; set; }

        /// <summary>
        /// 联系人名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 头像地址
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}