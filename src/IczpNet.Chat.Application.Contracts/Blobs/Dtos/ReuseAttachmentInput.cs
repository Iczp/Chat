using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Blobs.Dtos;

/// <summary>
/// 同账号多身份复用已上传附件请求
/// </summary>
public class ReuseAttachmentInput
{
    [Required]
    public string ReuseToken { get; set; }

    [MaxLength(64)]
    public string ClientUploadId { get; set; }

    [MaxLength(256)]
    public string FileName { get; set; }
}
