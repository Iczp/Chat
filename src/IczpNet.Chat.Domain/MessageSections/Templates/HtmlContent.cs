using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates
{
    public class HtmlContent : MessageContent
    {
        /// <summary>
        /// 编辑器类型
        /// </summary>
        public virtual EditorTypeEnum EditorType { get; set; }
        /// <summary>
        /// 文本内容
        /// </summary>
        //[Required(ErrorMessage = "文本内容[Title]必填！")]
        [StringLength(256)]
        //[Index]
        public virtual string Title { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [StringLength(5000)]
        public virtual string Content { get; set; }
        /// <summary>
        /// 原始地址
        /// </summary>
        [StringLength(500)]
        public virtual string OriginalUrl { get; set; }
    }
}
