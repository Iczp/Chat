using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections
{
    public interface IContentProvider
    {
        Task<IMessageContentInfo> GetContent(Guid messageId);

        Task<TOutput> Create<TInput, TOutput>(TInput input);
    }

}
