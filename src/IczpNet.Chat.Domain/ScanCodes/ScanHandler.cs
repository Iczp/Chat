using IczpNet.Chat.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.ScanCodes;

public class ScanHandler : BaseEntity<Guid>
{
    /// <summary>
    /// 
    /// </summary>
    public virtual Guid ScanCodeId {  get; set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(ScanCodeId))]
    public virtual ScanCode ScanCode { get; set; }

    /// <summary>
    /// Action
    /// </summary>
    [Comment("Action")]
    [StringLength(256)]
    public virtual string Action { get; set; }

    /// <summary>
    /// 条码类型
    /// </summary>
    [Comment("条码类型")]
    [StringLength(256)]
    public virtual string Handler { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [StringLength(256)]
    public string HandlerFullName { get; set; }

    /// <summary>
    /// 执行时间（毫秒）
    /// </summary>
    [Comment("执行时间（毫秒）")]
    public virtual double Execution { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    [Comment("消息")]
    [StringLength(256)]
    public virtual string Message { get; set; }

    /// <summary>
    /// 处理结果
    /// </summary>
    [Comment("处理结果")]
    [StringLength(5000)]
    public virtual string Result { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public virtual bool Success {  get; set; }


}
