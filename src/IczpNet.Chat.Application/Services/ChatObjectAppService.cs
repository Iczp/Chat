using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services
{
    public class ChatObjectAppService
        : CrudChatAppService<
            ChatObject,
            ChatObjectDetailDto,
            ChatObjectDto,
            Guid,
            ChatObjectGetListInput,
            ChatObjectCreateInput,
            ChatObjectUpdateInput>,
        IChatObjectAppService
    {
        public ChatObjectAppService(IRepository<ChatObject, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<ChatObject>> CreateFilteredQueryAsync(ChatObjectGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))

                ;
        }
    }
}
