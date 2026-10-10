using AutoMapper;
using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.MessageSections.Templates;
using Shouldly;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace IczpNet.Chat;

public class PlainMessageContentMapperly_Tests
{
    private readonly IMapper _legacyMapper;

    public PlainMessageContentMapperly_Tests()
    {
        var profile = typeof(ChatDomainModule).Assembly.GetType(
            "IczpNet.Chat.AutoMappers.ChatApplicationAutoMapperProfile", throwOnError: true)!;
        var configuration = new MapperConfiguration(config => config.AddProfile(profile));
        _legacyMapper = configuration.CreateMapper();
    }

    [Fact]
    public void Html_content_matches_for_create_update_and_output()
    {
        var mapper = new ChatDomainHtmlContentInfoToHtmlContentMapper();
        AssertParity(
            new HtmlContentInfo { Title = "first", Content = "<p>first</p>", OriginalUrl = "https://example.test/1" },
            new HtmlContentInfo { Title = "second", Content = "<p>second</p>", OriginalUrl = null },
            mapper.Map, mapper.Map, mapper.ReverseMap);
    }

    [Fact]
    public void Contacts_content_matches_for_create_update_and_output()
    {
        var mapper = new ChatDomainContactsContentInfoToContactsContentMapper();
        AssertParity(
            new ContactsContentInfo { DestinationId = 123, Name = "first", Code = "c1", Portrait = "p1" },
            new ContactsContentInfo { DestinationId = 456, Name = "second", Code = null, Portrait = "p2" },
            mapper.Map, mapper.Map, mapper.ReverseMap);
    }

    [Fact]
    public void Link_content_matches_for_create_update_and_output()
    {
        var mapper = new ChatDomainLinkContentInfoToLinkContentMapper();
        AssertParity(
            new LinkContentInfo { Title = "first", Url = "https://example.test/1", Image = "img1" },
            new LinkContentInfo { Title = "second", Url = "https://example.test/2", Image = null },
            mapper.Map, mapper.Map, mapper.ReverseMap);
    }

    [Fact]
    public void Location_content_matches_for_create_update_and_output()
    {
        var mapper = new ChatDomainLocationContentInfoToLocationContentMapper();
        AssertParity(
            new LocationContentInfo { Provider = "AMap", Name = "first", Latitude = 12.5f, Longitude = 21.5f },
            new LocationContentInfo { Provider = "Baidu", Name = "second", Latitude = 11.5f, Longitude = 22.5f },
            mapper.Map, mapper.Map, mapper.ReverseMap);
    }

    private void AssertParity<TInfo, TEntity>(
        TInfo source,
        TInfo update,
        Func<TInfo, TEntity> create,
        Action<TInfo, TEntity> mapToExisting,
        Func<TEntity, TInfo> output)
        where TInfo : class
        where TEntity : class
    {
        var expected = _legacyMapper.Map<TEntity>(source);
        var actual = create(source);
        AssertSameMappedFields(expected, actual);

        var expectedStamp = GetStamp(expected);
        var actualStamp = GetStamp(actual);
        _legacyMapper.Map(update, expected);
        mapToExisting(update, actual);
        AssertSameMappedFields(expected, actual);
        GetStamp(expected).ShouldBe(expectedStamp);
        GetStamp(actual).ShouldBe(actualStamp);

        var expectedOutput = _legacyMapper.Map<TInfo>(expected);
        var actualOutput = output(actual);
        JsonSerializer.Serialize(actualOutput).ShouldBe(JsonSerializer.Serialize(expectedOutput));
    }

    private static string GetStamp<T>(T entity) =>
        (string)typeof(T).GetProperty("ConcurrencyStamp")!.GetValue(entity)!;

    private static void AssertSameMappedFields<T>(T expected, T actual)
    {
        var expectedJson = JsonNode.Parse(JsonSerializer.Serialize(expected))!.AsObject();
        var actualJson = JsonNode.Parse(JsonSerializer.Serialize(actual))!.AsObject();
        expectedJson.Remove("ConcurrencyStamp");
        actualJson.Remove("ConcurrencyStamp");
        actualJson.ToJsonString().ShouldBe(expectedJson.ToJsonString());
    }
}
