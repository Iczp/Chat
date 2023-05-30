using AutoMapper;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.AutoMappers
{
    public class MessageOwnerDtoMapping : DomainService, IMappingAction<Message, MessageOwnerDto>, ITransientDependency
    {

        protected IReadedRecorderManager ReadedRecorderManager { get; }
        protected IOpenedRecorderManager OpenedRecorderManager { get; }
        protected IFavoritedRecorderManager FavoritedRecorderManager { get; }

        public MessageOwnerDtoMapping(
            IReadedRecorderManager readedRecorderManager, 
            IOpenedRecorderManager openedRecorderManager, 
            IFavoritedRecorderManager favoritedRecorderManager)
        {
            ReadedRecorderManager = readedRecorderManager;
            OpenedRecorderManager = openedRecorderManager;
            FavoritedRecorderManager = favoritedRecorderManager;
        }



        public void Process(Message source, MessageOwnerDto destination, ResolutionContext context)
        {
            Logger.LogInformation("MessageOwnerDtoMapping");

            //destination.KeyValue = "55555555555555555555555";
            //var id = source.Id;

            //source.IsReaded = ReadedRecorderManager.IsAnyAsync(id, e.Id).GetAwaiter().GetResult();
            //source.IsOpened = OpenedRecorderManager.IsAnyAsync(id, e.Id);
            //source.IsFavorited = FavoritedRecorderManager.IsAnyAsync(id, e.Id);
        }
    }
}
