using IczpNet.Chat.Enums;

namespace IczpNet.Chat.MessageSections.Templates;

public class RedEnvelopeContentInput : MessageContentInfoBase, IContentInfo//, IValidatableObject
{

    public virtual string Password { get; set; }
    /// <summary>
    /// 红包发放方式（0：随机金额;1:固定金额）
    /// </summary>
    public virtual GrantModes GrantMode { get; set; }

    /// <summary>
    /// 单个金额Red Envelope
    /// </summary>
    public virtual decimal Amount { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public virtual int Count { get; set; }

    /// <summary>
    /// 总金额
    /// </summary>
    public virtual decimal TotalAmount { get; set; }

    /// <summary>
    /// 文本内容
    /// </summary>
    public virtual string Text { get; set; }

    //public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    //{
    //    if (Count <= 0)
    //    {
    //        yield return new ValidationResult("红包数量出错", new[] { nameof(Count) });
    //    }
    //    if (TotalAmount <= 0)
    //    {
    //        yield return new ValidationResult("红包总金额出错", new[] { nameof(TotalAmount) });
    //        switch (Mode)
    //        {
    //            case GrantModes.FixedAmount:
    //                if (Amount < 0.01m)
    //                {
    //                    yield return new ValidationResult("单个金额不能小于1分钱！", new[] { nameof(Count) });
    //                }
    //                if (TotalAmount != Amount * Count)
    //                {
    //                    yield return new ValidationResult("定额红包总金额出错！", new[] { nameof(Count), nameof(TotalAmount) });
    //                }
    //                break;
    //            case GrantModes.RandomAmount:
    //                if (TotalAmount != Amount)
    //                {
    //                    yield return new ValidationResult("拼手气红包总金额与单个金额不相等", new[] { nameof(Count), nameof(TotalAmount) });
    //                }
    //                if (TotalAmount / Count < 0.01m)
    //                {
    //                    yield return new ValidationResult("红包最小金额不能小于1分钱", new[] { nameof(Count), nameof(TotalAmount) });
    //                }
    //                break;
    //        }
    //    }
    //}
}
