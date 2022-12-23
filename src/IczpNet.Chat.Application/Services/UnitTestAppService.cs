using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Services
{
    public class UnitTestAppService : ChatAppService
    {

        private static int TotalCount = 0;
        private static List<Guid> ChatObjectIdList;

        protected IRepository<Message, Guid> MessageRepository { get; }
        protected IRepository<Session, Guid> SessionRepository { get; }
        protected IRepository<ChatObject, Guid> ChatObjectRepository { get; }
        protected IMessageManager MessageManager { get; }

        protected IChatSender ChatSender { get; }
        public UnitTestAppService(
            IRepository<Message, Guid> messageRepository,
            IRepository<ChatObject, Guid> chatObjectRepository,
            IMessageManager messageManager,
            IChatSender chatSender,
            IRepository<Session, Guid> sessionRepository)
        {
            MessageRepository = messageRepository;
            ChatObjectRepository = chatObjectRepository;
            MessageManager = messageManager;
            ChatSender = chatSender;
            SessionRepository = sessionRepository;
        }
        public async Task<int> SendToEveryOneAsync(string text, Guid? receiverId, int count = 100)
        {

            if (TotalCount == 0)
            {
                var TotalCount = await ChatObjectRepository.CountAsync(x => x.ObjectType == ChatObjectTypes.Personal);
            }
            ChatObjectIdList ??= (await ChatObjectRepository.GetQueryableAsync())
                .Where(x => x.ObjectType == ChatObjectTypes.Personal)
                .Select(x => x.Id)
                .ToList();

            for (int i = 0; i < count; i++)
            {
                await ChatSender.SendTextMessageAsync(new MessageInput<TextContentInfo>()
                {
                    SenderId = ChatObjectIdList[new Random().Next(0, TotalCount)],
                    ReceiverId = receiverId ?? ChatObjectIdList[new Random().Next(0, TotalCount)],
                    Content = new TextContentInfo()
                    {
                        Text = i + ". " + text
                    }
                });
            }
            return count;
        }

        public Task<List<int>> GenerateIntAsync(int count, int minValue, int maxValue)
        {
            var items = new List<int>();

            var r = new Random();

            for (int i = 0; i < count; i++)
            {
                items.Add(r.Next(minValue, maxValue));
            }
            return Task.FromResult(items);
        }

        [UnitOfWork(true, System.Data.IsolationLevel.ReadUncommitted)]
        public async Task<int> SetSessionLastMessageAsync()
        {
            var items = await SessionRepository.GetListAsync(x => x.MessageList.Any());

            foreach (var item in items)
            {
                item.SetLastMessage(item.MessageList.OrderByDescending(x => x.AutoId).FirstOrDefault());
            }
            await SessionRepository.UpdateManyAsync(items);

            return items.Count;
        }
    }
}
