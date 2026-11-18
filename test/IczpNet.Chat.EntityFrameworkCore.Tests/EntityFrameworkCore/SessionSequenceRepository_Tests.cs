using System;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow;
using Xunit;

namespace IczpNet.Chat.EntityFrameworkCore;

public class SessionSequenceRepository_Tests : ChatTestBase<MessageQueryReviewTestModule>
{
    [Fact]
    public async Task Allocation_rejects_non_transactional_unit_of_work()
    {
        await WithUnitOfWorkAsync(new AbpUnitOfWorkOptions { IsTransactional = false }, async () =>
        {
            var repository = ServiceProvider.GetRequiredService<ISessionRepository>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdateLastMessageSeqAsync(Guid.NewGuid()));
        });
    }

    [Fact]
    public async Task Allocation_rolls_back_with_its_unit_of_work()
    {
        var id = Guid.NewGuid();
        await WithUnitOfWorkAsync(new AbpUnitOfWorkOptions { IsTransactional = true }, async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            db.Set<Session>().Add(new Session(id, Guid.NewGuid().ToString(), default(Channels)));
            await db.SaveChangesAsync();
        });
        using (var scope = ServiceProvider.CreateScope())
        {
            var manager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using var uow = manager.Begin(new AbpUnitOfWorkOptions { IsTransactional = true }, requiresNew: true);
            Assert.Equal(1L, await scope.ServiceProvider.GetRequiredService<ISessionRepository>().UpdateLastMessageSeqAsync(id));
            await uow.RollbackAsync();
        }
        await WithUnitOfWorkAsync(async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            Assert.Equal(0L, (await db.Set<Session>().SingleAsync(x => x.Id == id)).LastMessageSeq);
        });
    }

    [Fact]
    public async Task Allocation_updates_tracked_session_without_losing_other_changes()
    {
        await WithUnitOfWorkAsync(new AbpUnitOfWorkOptions { IsTransactional = true }, async () =>
        {
            var db = await ServiceProvider.GetRequiredService<IDbContextProvider<ChatDbContext>>().GetDbContextAsync();
            var session = new Session(Guid.NewGuid(), Guid.NewGuid().ToString(), default(Channels)) { Title = "before" };
            db.Set<Session>().Add(session);
            await db.SaveChangesAsync();
            var repository = ServiceProvider.GetRequiredService<ISessionRepository>();
            Assert.Equal(1L, await repository.UpdateLastMessageSeqAsync(session.Id));
            Assert.Equal(1L, session.LastMessageSeq);
            session.Title = "after";
            db.Update(session);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            var stored = await db.Set<Session>().SingleAsync(x => x.Id == session.Id);
            Assert.Equal(1L, stored.LastMessageSeq);
            Assert.Equal("after", stored.Title);
        });
    }
}
