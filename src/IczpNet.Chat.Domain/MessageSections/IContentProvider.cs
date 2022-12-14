using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections
{
    public interface IContentProvider
    {
        Task<IMessageContentInfo> GetContent(Guid messageId);
    }

    public interface IContentProvider<TInput, TOutput>
        where TInput : class
        where TOutput : class
    {
        Task<TOutput> Create(TInput input);
    }
}
