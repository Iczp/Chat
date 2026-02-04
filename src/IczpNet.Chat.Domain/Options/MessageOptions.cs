using Microsoft.Extensions.Caching.Distributed;
using System;

namespace IczpNet.Chat.Options;

public class MessageOptions
{
    public string OutputTempPath { get; set; } = "/media_temp";

    public DistributedCacheEntryOptions CacheOptions { get; set; } = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
    };

    public VideoOptions VideoSetting { get; set; } = new VideoOptions();

    public AudioOptions AudioSetting { get; set; } = new AudioOptions();

    public ImageOptions ImageSetting { get; set; } = new ImageOptions();

    public class VideoOptions
    {
        /// <summary>
        /// 是否生成视频快照
        /// </summary>
        public bool IsGenerateSnapshot { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>
        public int ImgCaptureSeconds { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        public (int Width, int Height) ImgSnapshotSize { get; set; } = (-1, -1);
        /// <summary>
        /// 
        /// </summary>
        public (int Width, int Height) GifSnapshotSize { get; set; } = (240, 160);
        /// <summary>
        /// 
        /// </summary>
        public int GifSeconds { get; set; } = 5;

        /// <summary>
        /// 快照略缩图大小
        /// </summary>
        public int SnapshotThumbnailSize { get; set; } = 160;

    }

    public class ImageOptions
    {
        /// <summary>
        /// 略缩图大小
        /// </summary>
        public int ThumbnailSize { get; set; } = 160;

        /// <summary>
        /// 大图大小
        /// </summary>
        public int BigSize { get; set; } = 1080;

    }

    public class AudioOptions
    {
    }



}
