using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Messages.Templates
{
    public class ContactsContent : MessageContent
    {
        /// <summary>
        /// 媒体Id
        /// </summary>
        [StringLength(36)]
        public string MediaId { get; set; }
        /// <summary>
        /// 媒体类型  0:未定义, 1:个人, 2:群, 3:订阅号, 4:公众号, 5:部门群, 6:课程群, 7:任务群
        /// </summary>
        public string MediaType { get; set; }
        /// <summary>
        /// 联系人名称
        /// </summary>
        [Required(ErrorMessage = "联系人名称[Title]必填")]
        //[Index]
        [StringLength(500)]
        public string Title { get; set; }
        /// <summary>
        /// 头像地址
        /// </summary>
        [StringLength(500)]
        public string Picture { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
        public string Remark { get; set; }
    }
}
