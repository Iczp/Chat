using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Uow;
using Xunit;

namespace IczpNet.Chat.SessionUnits;

public class DirtyCommit_Tests
{
    [Fact]
    public async Task Failed_database_commit_must_not_acknowledge_processing_records()
    {
        var cache = Substitute.For<ISessionUnitCacheManager>();
        var repository = Substitute.For<ISessionUnitRepository>();
        var uows = Substitute.For<IUnitOfWorkManager>();
        var uow = Substitute.For<IUnitOfWork>();
        uows.Begin(Arg.Any<AbpUnitOfWorkOptions>(), true).Returns(uow);
        uow.CompleteAsync(Arg.Any<CancellationToken>()).Returns(_ => Task.FromException(new InvalidOperationException("Simulated commit failure")));
        var distributedLock = Substitute.For<IAbpDistributedLock>();
        distributedLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<IAbpDistributedLockHandle>());
        var unit = new SessionUnitCacheItem { Id = Guid.NewGuid(), LastMessageId = 100 };
        var args = new FlushDirtyProcessingJobArgs
        {
            ProcessingKey = "isolated-processing", SessionUnitIds = [unit.Id],
            DirtyVersions = new Dictionary<string, long> { ["entry"] = 100 }
        };
        cache.HasPendingDirtyAsync(args).Returns(true);
        cache.GetManyAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new[] { new KeyValuePair<Guid, SessionUnitCacheItem>(unit.Id, unit) });
        repository.BatchUpdateAsync(Arg.Any<List<SessionUnitCacheItem>>()).Returns(1);
        var job = new FlushDirtyProcessingJob(cache, repository, uows, distributedLock);
        await Assert.ThrowsAsync<InvalidOperationException>(() => job.ExecuteAsync(args));
        await cache.DidNotReceiveWithAnyArgs().AcknowledgeDirtyAsync(default);
        await repository.Received(1).BatchUpdateAsync(Arg.Any<List<SessionUnitCacheItem>>());
    }
}
