//using IczpNet.Chat.Enums;
//using Volo.Abp.Data;
//using Volo.Abp.Identity;

//namespace IczpNet.Chat.Identitys;

//public static class IdentityUserExtensions
//{

//    #region ChatUserType

//    public const string ChatObjectTypePropertyName = "ChatObjectType";

//    public static void SetChatObjectType(this IdentityUser user, ChatObjectTypes  chatObjectType)
//    {
//        user.SetProperty(ChatObjectTypePropertyName, chatObjectType);
//    }

//    public static ChatObjectTypes? GetChatObjectType(this IdentityUser user)
//    {
//        return user.GetProperty<ChatObjectTypes?>(ChatObjectTypePropertyName);
//    }

//    #endregion
//}
