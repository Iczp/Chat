using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace IczpNet.Chat.Samples;

public class SampleAppService_Tests : ChatApplicationTestBase
{
    private readonly ISampleAppService _sampleAppService;

    public SampleAppService_Tests()
    {
        _sampleAppService = GetRequiredService<ISampleAppService>();
    }

    [Fact(Skip = "Template ISampleAppService has no implementation in Chat.Application.")]
    public async Task GetAsync()
    {
        var result = await _sampleAppService.GetAsync();
        result.Value.ShouldBe(42);
    }

    [Fact(Skip = "Template ISampleAppService has no implementation in Chat.Application.")]
    public async Task GetAuthorizedAsync()
    {
        var result = await _sampleAppService.GetAuthorizedAsync();
        result.Value.ShouldBe(42);
    }
}
