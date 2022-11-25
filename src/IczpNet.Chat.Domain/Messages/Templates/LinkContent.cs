using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Messages.Templates
{
    public class LinkContent : MessageContent
    {
        /// <summary>
        /// Url
        /// </summary>
        [Required(ErrorMessage = "链接[Url]必填")]
        [StringLength(500)]
        public virtual string Url { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        [Required(ErrorMessage = "链接显示名称 必填")]
        [StringLength(256)]
        //[Index]
        public virtual string Title { get; set; }
        /// <summary>
        /// 简要说明
        /// </summary>
        [StringLength(500)]
        //[Index]
        public virtual string Description { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        [StringLength(500)]
        public virtual string Image { get; set; }
        /// <summary>
        /// 发行人名称
        /// </summary>
        [StringLength(256)]
        //[Index]
        public virtual string IssuerName { get; set; }
        /// <summary>
        /// 发行人图标
        /// </summary>
        [StringLength(500)]
        public virtual string IssuerIcon { get; set; }
    }
}
