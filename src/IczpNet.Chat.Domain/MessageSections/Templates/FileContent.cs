﻿using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates;

[MessageTemplate(MessageTypes.File)]
[ContentOuput(typeof(FileContentInfo))]
public class FileContent : MessageContentAttachmentsEntityBase
{
    public override long GetSize() => Size ?? 0;

    /// <summary>
    /// 文件地址
    /// </summary>
    //[Required(ErrorMessage = "文件控制器地址")]
    [StringLength(500)]
    public virtual string ActionUrl { get; set; }
}
