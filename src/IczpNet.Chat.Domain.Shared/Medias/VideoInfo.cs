using System;

namespace IczpNet.Chat.Medias;

public class VideoInfo : AudioInfo
{
    /// <summary>
    /// Resolver
    /// </summary>
    public virtual string Resolver { get; set; }

    /// <summary>
    /// 视频封面Width
    /// </summary>
    public virtual int? Width { get; set; }

    /// <summary>
    /// 视频Height
    /// </summary>
    public virtual int? Height { get; set; }

    /// <summary>
    /// Snapshot Gif Path
    /// </summary>
    public virtual string GifSnapshotPath { get; set; }

    /// <summary>
    /// Snapshot Image Path
    /// </summary>
    public virtual string ImageSnapshotPath { get; set; }

    /// <summary>
    /// temp path
    /// </summary>
    public virtual string TempVideoPath { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual double Size { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual long? ElapsedMilliseconds { get; set; }
}
