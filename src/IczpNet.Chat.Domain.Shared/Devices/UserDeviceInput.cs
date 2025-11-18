using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Devices;
public class UserDeviceInput
{
    /// <summary>
    /// 
    /// </summary>
    [Required]
    public string RawDeviceId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string RawDeviceType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string UserName { get; set; }
}
