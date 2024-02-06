using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.ServiceStates;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectMapper //: DomainService, IObjectMapper<ChatObject, ChatObjectDto>, ITransientDependency
    {
        protected IServiceStateManager ServiceStateManager { get; set; }

        public ChatObjectMapper(IServiceStateManager serviceStateManager)
        {
            ServiceStateManager = serviceStateManager;
        }

        public ChatObjectDto Map(ChatObject source)
        {
            return new ChatObjectDto()
            {
                Name = "Mapper Test",
            };
        }

        public ChatObjectDto Map(ChatObject source, ChatObjectDto destination)
        {
            return destination;
        }
    }
}
