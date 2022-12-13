using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections
{
    public interface IContentProvider
    {
        string ProviderName { get; }
        Task<IMessageContentInfo> GetContent(Guid messageId);
    }
}
