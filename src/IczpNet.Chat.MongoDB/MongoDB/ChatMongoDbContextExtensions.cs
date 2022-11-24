using Volo.Abp;
using Volo.Abp.MongoDB;

namespace IczpNet.Chat.MongoDB;

public static class ChatMongoDbContextExtensions
{
    public static void ConfigureChat(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
    }
}
