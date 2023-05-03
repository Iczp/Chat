using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Volo.Abp.SimpleStateChecking;
using Volo.Abp.Users;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class SessionUnitStateChecker : ISimpleStateChecker<SessionUnit>
    {
        public virtual async Task<bool> IsEnabledAsync(SimpleStateCheckerContext<SessionUnit> context)
        {
            await Task.CompletedTask;
            //context.State.Name
            var currentUser = context.ServiceProvider.GetRequiredService<ICurrentUser>();

            return context.State.IsEnabled;
        }
    }
}
