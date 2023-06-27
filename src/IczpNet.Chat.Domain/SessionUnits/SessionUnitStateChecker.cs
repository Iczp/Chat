using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Volo.Abp.SimpleStateChecking;
using Volo.Abp.Users;

namespace IczpNet.Chat.SessionUnits
{
    public class SessionUnitStateChecker : ISimpleStateChecker<SessionUnit>
    {
        public virtual async Task<bool> IsEnabledAsync(SimpleStateCheckerContext<SessionUnit> context)
        {
            await Task.Yield();
            //context.State.Name
            var currentUser = context.ServiceProvider.GetRequiredService<ICurrentUser>();

            return context.State.Setting.IsEnabled;
        }
    }
}
