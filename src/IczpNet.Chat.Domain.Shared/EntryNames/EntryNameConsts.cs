using System.ComponentModel;
using Volo.Abp.Reflection;

namespace IczpNet.Chat.EntryNames;

public class EntryNameConsts
{
    private static string[] allNames;

    public static string[] GetAll()
    {
        allNames ??= ReflectionHelper.GetPublicConstantsRecursively(typeof(EntryNameConsts));
        return allNames;
    }

    [Description("手机")]
    public const string Phone = nameof(Phone);

    [Description("电话")]
    public const string Tel = nameof(Tel);

    [Description("Email")]
    public const string Email = nameof(Email);

    [Description("微信号")]
    public const string WeiChat = nameof(WeiChat);

    [Description("地址")]
    public const string Address = nameof(Address);

    [Description("性别")]
    public const string Gender = nameof(Gender);

    [Description("生日")]
    public const string Birthday = nameof(Birthday);

    [Description("公司")]
    public const string Company = nameof(Company);

    [Description("部门")]
    public const string Department = nameof(Department);

    [Description("职务")]
    public const string Position = nameof(Position);

    [Description("办公地址")]
    public const string WorkAddress = nameof(WorkAddress);

    [Description("网址")]
    public const string Website = nameof(Website);

    [Description("客户等级")]
    public const string CustomerLevel = nameof(CustomerLevel);

    [Description("备用电话")]
    public const string BackupPhone = nameof(BackupPhone);
}
