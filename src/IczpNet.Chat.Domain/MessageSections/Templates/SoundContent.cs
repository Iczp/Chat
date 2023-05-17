using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace IczpNet.Chat.MessageSections.Templates
{
    [MessageTemplate(MessageTypes.Sound)]
    [ContentOuput(typeof(SoundContentInfo))]
    public class SoundContent : MessageContentEntityBase
    {
        public override long GetSize() => Size ?? 0;
        /// <summary>
        /// 语音地址
        /// </summary>
        [Required(ErrorMessage = "语音地址必填")]
        [StringLength(500)]
        public virtual string Url { get; set; }
        /// <summary>
        /// 手机的存储路径(前端使用字段，服务端不能存这个字段)
        /// </summary>
        [StringLength(500)]
        public virtual string Path { get; set; }
        /// <summary>
        /// 语音的文本内容
        /// </summary>
        //[Index]
        [StringLength(500)]
        public virtual string Text { get; set; }
        /// <summary>
        /// 语音时长（毫秒）
        /// </summary>
        [Required(ErrorMessage = "语音时长（毫秒）必填")]
        public virtual int Time { get; set; }

        public virtual long? Size { get; set; }
    }
}
