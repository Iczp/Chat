using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.SessionRequests;

// Runs only in the standalone Chat Worker. Quartz persists the job in its own
// database; the manager rechecks Chat SQL and message idempotency on every retry.
public class SessionRequestNotificationJob(SessionRequestManager manager)
    : AsyncBackgroundJob<SessionRequestNotificationArgs>, ITransientDependency
{
    public override Task ExecuteAsync(SessionRequestNotificationArgs args) =>
        manager.DeliverNotificationAsync(args);
}
