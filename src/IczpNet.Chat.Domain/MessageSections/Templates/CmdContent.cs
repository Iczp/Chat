using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates;

[MessageTemplate(MessageTypes.Cmd)]
[ContentOuput(typeof(CmdContentInfo))]
public class CmdContent : MessageContentEntityBase, IUrl
{
    public override long GetSize() => System.Text.Encoding.Default.GetByteCount(Text + Url + Cmd);

    [StringLength(20)]
    public virtual string Cmd { get; set; }

    [StringLength(500)]
    public virtual string Text { get; set; }

    /// <summary>
    /// app:///pages/im/notice?id=123
    /// </summary>
    [StringLength(500)]
    public virtual string Url { get; set; }
}
