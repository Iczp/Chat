using System.ComponentModel;
using Volo.Abp.Reflection;

namespace IczpNet.Chat.EntryNames
{
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
    }
}
