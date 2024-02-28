namespace IczpNet.Chat.MessageSections.Templates;

/// <summary>
/// 视频消息
/// </summary>
public class VideoContentInfo : MessageContentAttachmentsInfoBase, IContentInfo
{
    /// <summary>
    /// 视频地址
    /// </summary>
    public override string Url { get; set; }

    /// <summary>
    /// 视频封面Width
    /// </summary>
    public virtual int Width { get; set; }

    /// <summary>
    /// 视频Height
    /// </summary>
    public virtual int Height { get; set; }

    /// <summary>
    /// 视频封面
    /// </summary>
    public virtual string ImageUrl { get; set; }

    /// <summary>
    /// 视频Width
    /// </summary>
    public virtual int ImageWidth { get; set; }

    /// <summary>
    /// Height
    /// </summary>
    public virtual int ImageHeight { get; set; }

    /// <summary>
    /// 封面大小
    /// </summary>
    public virtual double ImageSize { get; set; }

    /// <summary>
    /// 选定视频的时间长度，单位为 （毫秒）
    /// </summary>
    public virtual double Duration { get; set; }

}