using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Security.Dtos;

/// <summary>
/// URL 安全检测入参
/// </summary>
public class UrlCheckInput
{
    /// <summary>
    /// 待检测的完整 URL
    /// </summary>
    [Required]
    [StringLength(2048)]
    public string Url { get; set; }
}
