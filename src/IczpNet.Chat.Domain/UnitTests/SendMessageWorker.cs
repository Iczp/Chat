using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Connections
{
    public class SendMessageWorker : AsyncPeriodicBackgroundWorkerBase
    {
        private static List<Guid> ChatObjectIdList;

        protected IRepository<ChatObject, Guid> ChatObjectRepository { get; }
        protected IChatSender ChatSender { get; }

        public SendMessageWorker(AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory,
            IRepository<ChatObject, Guid> chatObjectRepository,
            IChatSender chatSender) : base(timer, serviceScopeFactory)
        {
            Timer.Period = 3 * 1000; //3 seconds
            ChatObjectRepository = chatObjectRepository;
            ChatSender = chatSender;
        }

        [UnitOfWork]
        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            var startTicks = DateTime.Now.Ticks;

            Logger.LogInformation($"SendMessageWorker running:{DateTime.Now}, Timer.Period:{Timer.Period}ms");

            var count = await SendToEveryOneAsync("SendMessageWorker", null, new Random().Next(80, 200));

            float ticks = (DateTime.Now.Ticks - startTicks) / 10000;

            Logger.LogInformation($"SendMessageWorker send message count:{count},run ticks:{ticks}ms");
        }

        protected async Task<int> SendToEveryOneAsync(string text, Guid? receiverId = null, int count = 100)
        {
            ChatObjectIdList ??= (await ChatObjectRepository.GetQueryableAsync())
                .Where(x => x.ObjectType == ChatObjectTypes.Personal)
                .Select(x => x.Id)
                .ToList();

            Logger.LogInformation($"SendMessageWorker ChatObjectIdList.Count:{ChatObjectIdList.Count}");

            for (int i = 0; i < count; i++)
            {
                await ChatSender.SendTextMessageAsync(new MessageInput<TextContentInfo>()
                {
                    SenderId = ChatObjectIdList[new Random().Next(0, ChatObjectIdList.Count)],
                    ReceiverId = receiverId ?? ChatObjectIdList[new Random().Next(0, ChatObjectIdList.Count)],
                    Content = new TextContentInfo()
                    {
                        Text = i + ". " + text
                    }
                });
            }

            return count;
        }
    }
}
