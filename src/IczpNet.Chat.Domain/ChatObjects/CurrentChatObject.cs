using IczpNet.Chat.Enums;
using System;
using System.Linq;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace IczpNet.Chat.ChatObjects
{
    public class CurrentChatObject : DomainService, ICurrentChatObject
    {
        private ICurrentUser CurrentUser { get; }

        private IChatObjectManager ChatObjectManager { get; }

        private ChatObject _chatObject;

        public ChatObject ChatObject
        {
            get => _chatObject ??= ChatObjectManager.GetAsync(Id).Result;
        }

        public CurrentChatObject(
            ICurrentUser currentUser,
            IChatObjectManager chatObjectManager)
        {
            CurrentUser = currentUser;
            ChatObjectManager = chatObjectManager;
        }

        public virtual Guid Id => GetId() ?? throw new Exception("Get [ChatObjectId] Exception");

        public virtual long AutoId => ChatObject.AutoId;

        public virtual string Name => ChatObject.Name;

        public virtual Guid? OwnerUserId => ChatObject.AppUserId;

        public virtual ChatObjectTypeEnums? ObjectType => ChatObject.ObjectType;

        public virtual Guid? GetId() => CurrentUser.GetChatObjectIdList().FirstOrDefault();

    }
}
