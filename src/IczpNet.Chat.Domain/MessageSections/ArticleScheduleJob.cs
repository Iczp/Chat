using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.MessageSections;

public class ArticleScheduleJobArgs
{
    public Guid Id { get; set; }
    public int Version { get; set; }
}

public class ArticleScheduleJob(ArticleScheduleManager manager) : AsyncBackgroundJob<ArticleScheduleJobArgs>, ITransientDependency
{
    public override Task ExecuteAsync(ArticleScheduleJobArgs args) => manager.ExecuteAsync(args);
}
