using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates
{
    [MessageTemplate(MessageTypes.Video)]
    [ContentOuput(typeof(VideoContentInfo))]
    public class VideoContent : MessageContentEntityBase
    {
        /// <summary>
        /// 视频地址
        /// </summary>
        [Required(ErrorMessage = "视频地址必填")]
        [StringLength(256)]
        public virtual string Url { get; set; }

        /// <summary>
        /// 视频封面Width
        /// </summary>
        public virtual int? Width { get; set; }

        /// <summary>
        /// 视频Height
        /// </summary>
        public virtual int? Height { get; set; }

        /// <summary>
        /// 视频大小
        /// </summary>
        public virtual int? Size { get; set; }

        /// <summary>
        /// 视频封面
        /// </summary>
        [StringLength(256)]
        public virtual string ImageUrl { get; set; }

        /// <summary>
        /// 视频Width
        /// </summary>
        public virtual int? ImageWidth { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        public virtual int? ImageHeight { get; set; }

        /// <summary>
        /// 封面大小
        /// </summary>
        public virtual int? ImageSize { get; set; }

        /// <summary>
        /// 选定视频的时间长度，单位为 （毫秒）
        /// </summary>
        public virtual int? Duration { get; set; }
    }
}
