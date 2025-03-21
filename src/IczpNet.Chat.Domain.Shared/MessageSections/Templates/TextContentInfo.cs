﻿namespace IczpNet.Chat.MessageSections.Templates;

/// <summary>
/// 文本消息
/// </summary>
public class TextContentInfo : MessageContentInfoBase, IContentInfo
{
    /// <summary>
    /// 文本内容
    /// </summary>
    //[Required(ErrorMessage = "文本内容[Text]必填！")]
    public virtual string Text { get; set; }

}