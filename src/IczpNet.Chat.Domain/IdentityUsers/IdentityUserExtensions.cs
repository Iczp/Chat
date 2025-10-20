//using IczpNet.Chat.Enums;
//using Microsoft.AspNetCore.Identity;
//using Volo.Abp.Data;

//namespace IczpNet.Chat.IdentityUsers;

//public static class IdentityUserExtensions
//{

//    #region ChatUserType

//    public const string ChatObjectTypePropertyName = "ChatObjectType";

//    public static void SetChatObjectType(this IdentityUser user, ChatObjectTypeEnums chatObjectType)
//    {
//        user.SetProperty(ChatObjectTypePropertyName, chatObjectType);
//    }

//    public static ChatObjectTypeEnums? GetChatObjectType(this IdentityUser user)
//    {
//        return user.GetProperty<ChatObjectTypes?>(ChatObjectTypePropertyName);
//    }

//    #endregion
//}
