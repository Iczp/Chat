using IczpNet.Chat.MessageSections.Messages;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageSections
{
    public class MessageValidator : DomainService, IMessageValidator
    {
        public virtual Task CheckAsync(Message entity)
        {
            return Task.CompletedTask;
        }
    }
}
