using System;
using Volo.Abp.Users;

namespace IczpNet.Chat.ChatObjects
{
    public static class CurrentUserExtention
    {

        public static Guid? GetChatObjectId(this ICurrentUser currentUser)
        {
            var chatObjectIdValue = currentUser.FindClaim(ChatClaims.Id);

            if (chatObjectIdValue == null)
            {
                return null;
            }

            if (Guid.TryParse(chatObjectIdValue.Value, out Guid chatObjectId))
            {
                return chatObjectId;
            }

            return null;
        }
    }
}
