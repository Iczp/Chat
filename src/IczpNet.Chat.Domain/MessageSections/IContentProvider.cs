using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections
{
    public interface IContentProvider
    {
        Task<IContentInfo> GetContent(long messageId);

        Task<TOutput> Create<TInput, TOutput>(TInput input);
    }

}
