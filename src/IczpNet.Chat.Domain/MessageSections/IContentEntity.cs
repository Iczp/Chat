using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.DataFilters;

namespace IczpNet.Chat.MessageSections
{
    public interface IContentEntity : IContent, IIsEnabled, IChatOwner<long?>
    {
        bool IsVerified { get; }

        string GetBody();

        long GetSize();

        void SetOwnerId(long? ownerId);
    }
}
