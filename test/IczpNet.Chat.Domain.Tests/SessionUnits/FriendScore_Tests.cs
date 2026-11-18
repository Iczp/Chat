using IczpNet.Chat.SessionUnits;
using Xunit;

namespace IczpNet.Chat.SessionUnits;

public class FriendScore_Tests
{
    [Fact]
    public void Parse_should_preserve_the_original_score()
    {
        var source = FriendScore.Create(3, 1_726_000_000_123);

        var result = FriendScore.Parse(source.Value);

        Assert.Equal(source.Value, result.Value);
        Assert.Equal(3, result.Sorting);
        Assert.Equal(1_726_000_000_123, result.Ticks);
    }
}
