using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.TextContentWords;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections.Templates;

[MessageTemplate(MessageTypes.Text)]
[ContentOuput(typeof(TextContentInfo))]
public class TextContent : MessageContentEntityBase
{
    protected TextContent() { }

    public TextContent(Guid id) : base(id) { }

    [StringLength(ChatConsts.TextContentMaxLength)]
    public virtual string Text { get; set; }

    public override string GetBody() => FormatString(Text);

    public override long GetSize() => System.Text.Encoding.Default.GetByteCount(Text);

    [InverseProperty(nameof(TextContentWord.TextContent))]
    public virtual IList<TextContentWord> TextContentWordList { get; protected set; } = [];
}
