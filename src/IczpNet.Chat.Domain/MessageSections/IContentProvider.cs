using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections
{
    public interface IContentProvider
    {
        string ProviderName { get; }

        Task<IContentInfo> GetContentInfoAsync(long messageId);

        Task<TOutput> CreateAsync<TInput, TOutput>(TInput input);
    }

}
