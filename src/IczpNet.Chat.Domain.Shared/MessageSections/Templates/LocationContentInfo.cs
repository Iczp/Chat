namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 位置信息
    /// </summary>
    public class LocationContentInfo : BaseMessageContentInfo, IMessageContentInfo
    {
        /// <summary>
        /// AMap(高德地图)、baidu(百度地图)
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// 位置名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 简要说明
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 地图图片
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// 坐标 Latitude
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// 坐标 Longitude
        /// </summary>
        public float Longitude { get; set; }
    }
}