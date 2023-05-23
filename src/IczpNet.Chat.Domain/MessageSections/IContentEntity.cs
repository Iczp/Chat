using IczpNet.AbpCommons.DataFilters;

namespace IczpNet.Chat.MessageSections
{
    public interface IContentEntity : IContent, IIsEnabled
    {
        bool IsVerified { get; }

        string GetBody();

        long GetSize();
    }
}
