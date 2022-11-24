using System.Threading.Tasks;
using IczpNet.Chat.BaseAppServices;
using Microsoft.AspNetCore.Authorization;

namespace IczpNet.Chat.Samples;

public class SampleAppService : ChatAppService, ISampleAppService
{
    public Task<SampleDto> GetAsync()
    {
        return Task.FromResult(
            new SampleDto
            {
                Value = 42
            }
        );
    }

    [Authorize]
    public Task<SampleDto> GetAuthorizedAsync()
    {
        return Task.FromResult(
            new SampleDto
            {
                Value = 42
            }
        );
    }
}
