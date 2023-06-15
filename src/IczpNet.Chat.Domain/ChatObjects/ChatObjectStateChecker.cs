using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Volo.Abp.SimpleStateChecking;
using Volo.Abp.Users;

namespace IczpNet.Chat.ChatObjects
{
    public class ChatObjectStateChecker : ISimpleStateChecker<ChatObject>
    {
        public virtual async Task<bool> IsEnabledAsync(SimpleStateCheckerContext<ChatObject> context)
        {
            await Task.Yield();
            //context.State.Name
            var currentUser = context.ServiceProvider.GetRequiredService<ICurrentUser>();

            return context.State.IsEnabled;
        }
    }
}
