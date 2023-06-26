using IczpNet.Chat.BaseDtos;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.OpenedRecorders.Dtos
{
    public class GetListByMessageIdInput : GetListInput
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        [Required]
        public virtual long MessageId { get; set; }
        /// <summary>
        /// 是否已读
        /// </summary>
        [DefaultValue(true)]
        public virtual bool IsReaded { get; set; }
    }
}
