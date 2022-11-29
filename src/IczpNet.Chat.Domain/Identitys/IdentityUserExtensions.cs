using IczpNet.Chat.Enums;
using Volo.Abp.Data;
using Volo.Abp.Identity;

namespace IczpNet.Chat.Identitys;

public static class IdentityUserExtensions
{

    #region ChatUserType

    public const string ChatUserTypePropertyName = "UserType";

    public static void SetUserType(this IdentityUser user, ChatObjectTypeEnum  chatObjectType)
    {
        user.SetProperty(ChatUserTypePropertyName, chatObjectType);
    }

    public static ChatObjectTypeEnum GetUserType(this IdentityUser user)
    {
        return user.GetProperty<ChatObjectTypeEnum>(ChatUserTypePropertyName);
    }

    #endregion
}
