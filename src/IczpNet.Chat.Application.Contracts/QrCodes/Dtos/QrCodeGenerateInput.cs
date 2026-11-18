using IczpNet.Chat.Enums;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.QrCodes.Dtos;

public class QrCodeGenerateInput
{
    /// <summary>
    /// 所属聊天对象Id（名片对象或群聊Id；自定义场景可为空）
    /// </summary>
    public virtual long? OwnerId { get; set; }

    /// <summary>
    /// 二维码场景（不传时按 OwnerId 对象类型自动推断）
    /// </summary>
    public virtual QrCodeScenes? Scene { get; set; }

    /// <summary>
    /// 邀请人/分享人聊天对象Id（多ChatObjectId场景下必传或自动选择）
    /// </summary>
    public virtual long? InviterId { get; set; }

    /// <summary>
    /// 是否永久有效（群聊强制为临时码；名片不传ExpireDays且不为false时默认永久）
    /// </summary>
    public virtual bool? IsPermanent { get; set; }

    /// <summary>
    /// 有效天数（null表示永久或使用场景默认天数；可传 7 / 30 等自定义天数）
    /// </summary>
    [Range(1, 3650)]
    public virtual int? ExpireDays { get; set; }

    /// <summary>
    /// 是否强制重置（作废旧码并签发新码）
    /// </summary>
    public virtual bool ForceReset { get; set; }

    /// <summary>
    /// 标题/备注
    /// </summary>
    [MaxLength(200)]
    public virtual string Title { get; set; }

    /// <summary>
    /// 最大使用次数（空表示不限制）
    /// </summary>
    public virtual int? MaxUseCount { get; set; }

    /// <summary>
    /// 扩展业务载荷JSON
    /// </summary>
    [MaxLength(2000)]
    public virtual string PayloadJson { get; set; }
}
