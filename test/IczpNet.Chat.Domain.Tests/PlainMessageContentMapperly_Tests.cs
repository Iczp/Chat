using IczpNet.Chat.AutoMappers;
using IczpNet.Chat.MessageSections.Templates;
using Shouldly;
using Xunit;

namespace IczpNet.Chat;

public class PlainMessageContentMapperly_Tests
{
    [Fact]
    public void Html_content_create_update_and_output_work_properly()
    {
        var mapper = new ChatDomainHtmlContentInfoToHtmlContentMapper();
        var source = new HtmlContentInfo { Title = "first", Content = "<p>first</p>", OriginalUrl = "https://example.test/1" };
        var entity = mapper.Map(source);

        entity.Title.ShouldBe("first");
        entity.Content.ShouldBe("<p>first</p>");
        entity.OriginalUrl.ShouldBe("https://example.test/1");

        var update = new HtmlContentInfo { Title = "second", Content = "<p>second</p>", OriginalUrl = null };
        mapper.Map(update, entity);
        entity.Title.ShouldBe("second");
        entity.Content.ShouldBe("<p>second</p>");
        entity.OriginalUrl.ShouldBeNull();

        var output = mapper.ReverseMap(entity);
        output.Title.ShouldBe("second");
        output.Content.ShouldBe("<p>second</p>");
        output.OriginalUrl.ShouldBeNull();
    }

    [Fact]
    public void Contacts_content_create_update_and_output_work_properly()
    {
        var mapper = new ChatDomainContactsContentInfoToContactsContentMapper();
        var source = new ContactsContentInfo { DestinationId = 123, Name = "first", Code = "c1", Portrait = "p1" };
        var entity = mapper.Map(source);

        entity.DestinationId.ShouldBe(123);
        entity.Name.ShouldBe("first");
        entity.Code.ShouldBe("c1");
        entity.Portrait.ShouldBe("p1");

        var update = new ContactsContentInfo { DestinationId = 456, Name = "second", Code = null, Portrait = "p2" };
        mapper.Map(update, entity);
        entity.DestinationId.ShouldBe(456);
        entity.Name.ShouldBe("second");
        entity.Code.ShouldBeNull();
        entity.Portrait.ShouldBe("p2");

        var output = mapper.ReverseMap(entity);
        output.DestinationId.ShouldBe(456);
        output.Name.ShouldBe("second");
        output.Code.ShouldBeNull();
        output.Portrait.ShouldBe("p2");
    }

    [Fact]
    public void Link_content_create_update_and_output_work_properly()
    {
        var mapper = new ChatDomainLinkContentInfoToLinkContentMapper();
        var source = new LinkContentInfo { Title = "first", Url = "https://example.test/1", Image = "img1" };
        var entity = mapper.Map(source);

        entity.Title.ShouldBe("first");
        entity.Url.ShouldBe("https://example.test/1");
        entity.Image.ShouldBe("img1");

        var update = new LinkContentInfo { Title = "second", Url = "https://example.test/2", Image = null };
        mapper.Map(update, entity);
        entity.Title.ShouldBe("second");
        entity.Url.ShouldBe("https://example.test/2");
        entity.Image.ShouldBeNull();

        var output = mapper.ReverseMap(entity);
        output.Title.ShouldBe("second");
        output.Url.ShouldBe("https://example.test/2");
        output.Image.ShouldBeNull();
    }

    [Fact]
    public void Location_content_create_update_and_output_work_properly()
    {
        var mapper = new ChatDomainLocationContentInfoToLocationContentMapper();
        var source = new LocationContentInfo { Provider = "AMap", Name = "first", Latitude = 12.5f, Longitude = 21.5f };
        var entity = mapper.Map(source);

        entity.Provider.ShouldBe("AMap");
        entity.Name.ShouldBe("first");
        entity.Latitude.ShouldBe(12.5f);
        entity.Longitude.ShouldBe(21.5f);

        var update = new LocationContentInfo { Provider = "Baidu", Name = "second", Latitude = 11.5f, Longitude = 22.5f };
        mapper.Map(update, entity);
        entity.Provider.ShouldBe("Baidu");
        entity.Name.ShouldBe("second");
        entity.Latitude.ShouldBe(11.5f);
        entity.Longitude.ShouldBe(22.5f);

        var output = mapper.ReverseMap(entity);
        output.Provider.ShouldBe("Baidu");
        output.Name.ShouldBe("second");
        output.Latitude.ShouldBe(11.5f);
        output.Longitude.ShouldBe(22.5f);
    }
}
