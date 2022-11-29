using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates
{
    public class LocationContent : MessageContent
    {
        /// <summary>
        /// AMap(高德地图)、baidu(百度地图)
        /// </summary>
        [StringLength(100)]
        public virtual string Provider { get; set; }
        /// <summary>
        /// 位置名称
        /// </summary>
        [Required(ErrorMessage = "位置名称[Name]必填")]
        [StringLength(100)]
        public virtual string Name { get; set; }
        /// <summary>
        /// 简要说明
        /// </summary>
        [StringLength(200)]
        public virtual string Address { get; set; }
        /// <summary>
        /// 地图图片
        /// </summary>
        [StringLength(500)]
        public virtual string Image { get; set; }
        /// <summary>
        /// 坐标 Latitude
        /// </summary>
        public virtual float Latitude { get; set; }
        /// <summary>
        /// 坐标 Longitude
        /// </summary>
        public virtual float Longitude { get; set; }
    }
}
