using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.QrCodes.Dtos;

public class QrCodeApplyInput
{
    /// <summary>
    /// 二维码短码或完整二维码内容（如 gotoim://qr/{code}）
    /// </summary>
    [Required]
    [MaxLength(1024)]
    public virtual string Code { get; set; }

    /// <summary>
    /// 扫码人当前使用的聊天身份 ChatObjectId（多身份时必传或自动取默认身份）
    /// </summary>
    public virtual long? ApplicantChatObjectId { get; set; }

    /// <summary>
    /// 附加验证消息
    /// </summary>
    [MaxLength(200)]
    public virtual string RequestMessage { get; set; }
}
