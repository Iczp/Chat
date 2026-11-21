using System;
using System.Linq;
using IczpNet.Chat.CallCenters;
using IczpNet.Chat.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IczpNet.Chat.EntityFrameworkCore;

public class CallCenterPaginationSql_Tests
{
    [Fact]
    public void SqlServer_translates_assignment_cursor_and_queue_position()
    {
        using var context = new ChatDbContext(new DbContextOptionsBuilder<ChatDbContext>()
            .UseSqlServer("Server=localhost;Database=SqlGenerationOnly;Integrated Security=true;TrustServerCertificate=true")
            .Options);
        var afterAt = DateTime.UtcNow;
        var afterId = Guid.NewGuid();
        var query = context.Set<CallCenterAssignment>().IgnoreQueryFilters()
            .Where(x => x.ShopKeeperId == 1 && x.Status == CallCenterAssignmentStatus.Queued &&
                (x.QueuedAt > afterAt || (x.QueuedAt == afterAt && x.Id.CompareTo(afterId) > 0)))
            .OrderBy(x => x.QueuedAt).ThenBy(x => x.Id).Take(31);
        Assert.Contains("ORDER BY", query.ToQueryString());
    }
}
