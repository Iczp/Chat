using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace IczpNet.Chat.MongoDB;

[ConnectionStringName(ChatDbProperties.ConnectionStringName)]
public interface IChatMongoDbContext : IAbpMongoDbContext
{
    /* Define mongo collections here. Example:
     * IMongoCollection<Question> Questions { get; }
     */
}
