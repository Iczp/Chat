using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates
{
    public class ArticleContent : MessageContent
    {
        /// <summary>
        /// 文章类型
        /// </summary>
        public virtual ArticleTypeEnum ArticleType { get; set; }
        /// <summary>
        /// 编辑器类型
        /// </summary>
        //[Index]
        public virtual EditorTypeEnum EditorType { get; set; }
        /// <summary>
        /// 文本内容
        /// </summary>
        [Required(ErrorMessage = "文本内容[Title]必填！")]
        [StringLength(256)]
        //[Index]
        public virtual string Title { get; set; }
        /// <summary>
        /// 简要说明
        /// </summary>
        [StringLength(256)]
        //[Index]
        public virtual string Description { get; set; }
        /// <summary>
        /// 封面图片地址
        /// </summary>
        [StringLength(500)]
        public virtual string CoverImageUrl { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        [StringLength(5000)]
        public virtual string Content { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        [StringLength(50)]
        //[Index]
        public virtual string Author { get; set; }
        /// <summary>
        /// 访问数量
        /// </summary>
        public virtual long VisitsCount { get; set; }

        /// <summary>
        /// 原始地址
        /// </summary>
        [StringLength(500)]
        public virtual string OriginalUrl { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        [StringLength(36)]
        public virtual string CreatorUserId { get; set; }
    }
}
