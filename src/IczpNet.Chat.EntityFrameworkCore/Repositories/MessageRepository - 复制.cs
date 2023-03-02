using IczpNet.Chat.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using IczpNet.Chat.ChatObjects;

namespace IczpNet.Chat.Repositories
{
    public class ChatObjectRepository : ChatRepositoryBase<ChatObject, long>, IChatObjectRepository
    {
        public ChatObjectRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : base(dbContextProvider)
        {
        }
    }
}
