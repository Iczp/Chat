using IczpNet.Chat.RedisMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace IczpNet.Chat.RedisMapping;

public class RedisMapper_Tests : ChatDomainTestBase
{
    [Fact]
    public void Should_round_trip_nested_objects_and_complex_collections()
    {
        var source = new CacheModel
        {
            Name = "session",
            Detail = new DetailModel { Level = 2, UpdatedAt = new DateTime(2026, 9, 14, 8, 30, 0, DateTimeKind.Utc) },
            Members =
            [
                new DetailModel { Level = 3, UpdatedAt = new DateTime(2026, 9, 14, 8, 31, 0, DateTimeKind.Utc) },
                new DetailModel { Level = 4, UpdatedAt = new DateTime(2026, 9, 14, 8, 32, 0, DateTimeKind.Utc) },
            ],
            MembersByName = new Dictionary<string, DetailModel>
            {
                ["owner"] = new DetailModel { Level = 5, UpdatedAt = new DateTime(2026, 9, 14, 8, 33, 0, DateTimeKind.Utc) },
            },
        };

        var entries = source.ToHashEntries();
        var result = entries.ToObject<CacheModel>();

        Assert.Contains(entries, x => x.Name == "Members[0].Level");
        Assert.Contains(entries, x => x.Name == "MembersByName[owner].Level");
        Assert.Equal(source.Name, result.Name);
        Assert.Equal(source.Detail.Level, result.Detail.Level);
        Assert.Equal(source.Members.Select(x => x.Level), result.Members.Select(x => x.Level));
        Assert.Equal(source.MembersByName["owner"].Level, result.MembersByName["owner"].Level);
    }

    [Fact]
    public void Should_read_legacy_json_list_and_dictionary_fields()
    {
        var entries = new[]
        {
            new StackExchange.Redis.HashEntry("Ids", "[1,2,3]"),
            new StackExchange.Redis.HashEntry("Names", "{\"owner\":\"Alice\"}"),
        };

        var result = entries.ToObject<LegacyCollectionModel>();

        Assert.Equal(new long[] { 1, 2, 3 }, result.Ids);
        Assert.Equal("Alice", result.Names["owner"]);
    }

    public class CacheModel
    {
        public string Name { get; set; }
        public DetailModel Detail { get; set; }
        public List<DetailModel> Members { get; set; }
        public Dictionary<string, DetailModel> MembersByName { get; set; }
    }

    public class DetailModel
    {
        public int Level { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class LegacyCollectionModel
    {
        public List<long> Ids { get; set; }
        public Dictionary<string, string> Names { get; set; }
    }
}
