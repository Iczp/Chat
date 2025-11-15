using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace IczpNet.Chat.AppVersions;

///<summary>
/// 新增 
///</summary>
[Serializable]
public class AppVersionCreateInput: AppVersionUpdateInput
{
    /// <summary>
    /// 适用平台 ios | android | electron
    /// </summary>
    [MaxLength(50)]
    [Required]
    public virtual string Platform { get; set; }
}