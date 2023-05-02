using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.TextTemplates;
using Microsoft.AspNetCore.Mvc;
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
        private static List<long> ChatObjectIdList;

        protected IMessageRepository MessageRepository { get; }
        protected IRepository<Session, Guid> SessionRepository { get; }
        protected IChatObjectRepository ChatObjectRepository { get; }
        protected IMessageManager MessageManager { get; }
        protected IMessageSender ChatSender { get; }
        protected ISessionUnitIdGenerator SessionUnitIdGenerator { get; }
        public UnitTestAppService(
            IMessageRepository messageRepository,
            IChatObjectRepository chatObjectRepository,
            IMessageManager messageManager,
            IMessageSender chatSender,
            IRepository<Session, Guid> sessionRepository,
            ISessionUnitIdGenerator sessionUnitIdGenerator)
        {
            MessageRepository = messageRepository;
            ChatObjectRepository = chatObjectRepository;
            MessageManager = messageManager;
            ChatSender = chatSender;
            SessionRepository = sessionRepository;
            SessionUnitIdGenerator = sessionUnitIdGenerator;
        }
        public async Task<int> SendToEveryOneAsync(string text, long? receiverId, int count = 100)
        {

            if (TotalCount == 0)
            {
                TotalCount = await ChatObjectRepository.CountAsync(x => x.ObjectType == ChatObjectTypeEnums.Personal);
            }
            ChatObjectIdList ??= (await ChatObjectRepository.GetQueryableAsync())
                .Where(x => x.ObjectType == ChatObjectTypeEnums.Personal)
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
                item.SetLastMessage(item.MessageList.OrderByDescending(x => x.Id).FirstOrDefault());
            }
            await SessionRepository.UpdateManyAsync(items);

            return items.Count;
        }

        [HttpPost]
        public virtual Task<string> TextTemplateAsync(string template, Dictionary<string, object> data)
        {
            return Task.FromResult(new TextTemplate(template, data).ToString());
        }

        [HttpGet]
        public virtual Task<long> StringToIntAsync(string v, int length = 36)
        {
            return Task.FromResult(IntStringHelper.StringToInt(v, length));
        }

        [HttpGet]
        public virtual Task<string> IntToStringAsync(long v, int length = 36)
        {
            return Task.FromResult(IntStringHelper.IntToString(v, length));
        }

        [HttpPost]
        public virtual Task<string> SessionUnitIdGenerateAsync(long ownerId, long destinationId)
        {
            return Task.FromResult(SessionUnitIdGenerator.Generate(ownerId, destinationId));
        }


        [HttpPost]
        public virtual Task<Dictionary<string, long[]>> SessionUnitIdGenerateByRandomAsync(long count = 50, long maxValue = 123456)
        {
            var result = new Dictionary<string, long[]>();

            var rand = new Random();

            for (int i = 0; i < count; i++)
            {
                var ownerId = rand.NextInt64(maxValue);
                var destinationId = rand.NextInt64(maxValue);
                var ret = SessionUnitIdGenerator.Generate(ownerId, destinationId);
                result.Add(ret, new long[] { ownerId, destinationId });
            }
            return Task.FromResult(result);
        }

        [HttpPost]
        public virtual Task<long[]> SessionUnitIdResolvingAsync(string sessionUnitId)
        {
            return Task.FromResult(SessionUnitIdGenerator.Resolving(sessionUnitId));
        }
    }
}
