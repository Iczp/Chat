using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates
{
    public class HistoryContent : MessageContent
    {
        /// <summary>
        /// 文本内容
        /// </summary>
        [Required(ErrorMessage = "标题内容[Title]必填！")]
        [StringLength(256)]
        //[Index]
        public virtual string Title { get; set; }

        /// <summary>
        /// 简要说明
        /// </summary>
        [StringLength(500)]
        //[Index]
        public virtual string Description { get; set; }

        public virtual List<HistoryMessage> HistoryMessageList { set; get; }
    }
}
