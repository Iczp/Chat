using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates
{
    [MessageTemplate(MessageTypes.Image)]
    [ContentOuput(typeof(ImageContentInfo))]
    public class ImageContent : MessageContentAttachmentsEntityBase
    {
        public override long GetSize() => Size ?? 0;
        /// <summary>
        /// 图片地址
        /// </summary>
        //[Required(ErrorMessage = "图片地址[Url]必填")] 
        [StringLength(500)]
        public override string Url { get; set; }
        /// <summary>
        /// MinIO控制器URL
        /// </summary>
        [StringLength(500)]
        public virtual string ActionUrl { get; set; }
        /// <summary>
        /// 缩略图地址
        /// </summary>
        [StringLength(500)]
        public virtual string ThumbnailUrl { get; set; }
        /// <summary>
        /// 缩略图MinIO控制器URL
        /// </summary>
        [StringLength(500)]
        public virtual string ThumbnailActionUrl { get; set; }
        /// <summary>
        /// 拍照时设备方向信息 http://www.html5plus.org/doc/zh_cn/io.html#plus.io.ImageInfo
        /// </summary>
        [StringLength(36)]
        public virtual string Orientation { get; set; }
        /// <summary>
        /// Width
        /// </summary>
        public virtual int? Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public virtual int? Height { get; set; }
        /// <summary>
        /// 二维码信息
        /// </summary>
        [StringLength(500)]
        public virtual string Qrcode { get; set; }
    }
}
