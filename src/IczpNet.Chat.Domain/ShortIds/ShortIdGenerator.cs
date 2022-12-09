using Microsoft.Extensions.Options;
using shortid;
using shortid.Configuration;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ShortIds
{
    public class ShortIdGenerator : DomainService, IShortIdGenerator
    {
        protected ShortIdOptions Config { get; }

        public ShortIdGenerator(IOptions<ShortIdOptions> options)
        {
            Config = options.Value;
        }

        protected GenerationOptions GetOptions()
        {
            return (Config != null)
                ? new GenerationOptions(Config.UseNumbers, Config.UseSpecialCharacters, Config.Length)
                : new GenerationOptions(useSpecialCharacters: false);
        }

        public string Make()
        {
            return ShortId.Generate(GetOptions());
        }

        public Task<string> MakeAsync()
        {
            return Task.FromResult(Make());
        }
    }
}
