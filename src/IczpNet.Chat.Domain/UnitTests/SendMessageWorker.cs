using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.MessageSections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using IczpNet.Chat.ChatObjects;
using System.Collections.Generic;
using Volo.Abp.Domain.Repositories;
using IczpNet.Chat.Enums;
using System.Linq;

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

            var count = await SendToEveryOneAsync("SendMessageWorker", Guid.Parse("DF252895-3EC5-48C0-81D1-2DE149B51F1B"), 100);

            double ticks = (DateTime.Now.Ticks - startTicks); /// 10000;

            Logger.LogInformation($"SendMessageWorker send message count:{count},run ticks:{ticks}");
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
