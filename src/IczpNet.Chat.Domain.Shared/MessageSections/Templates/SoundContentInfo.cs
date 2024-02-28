namespace IczpNet.Chat.MessageSections.Templates;

/// <summary>
/// 语音消息
/// </summary>
public class SoundContentInfo : MessageContentAttachmentsInfoBase, IContentInfo
{
    /// <summary>
    /// 语音地址
    /// </summary>
    public override string Url { get; set; }

    /// <summary>
    /// 手机的存储路径(前端使用字段，服务端不能存这个字段)
    /// </summary>

    public virtual string Path { get; set; }

    /// <summary>
    /// 语音的文本内容
    /// </summary>
    public virtual string Text { get; set; }

    /// <summary>
    /// 语音时长（毫秒）
    /// </summary>
    public virtual int Time { get; set; }

}