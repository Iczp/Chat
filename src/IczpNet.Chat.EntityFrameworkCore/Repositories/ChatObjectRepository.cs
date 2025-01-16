using IczpNet.Chat.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using IczpNet.Chat.ChatObjects;

namespace IczpNet.Chat.Repositories;

public class ChatObjectRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : ChatRepositoryBase<ChatObject, long>(dbContextProvider), IChatObjectRepository
{
}
