using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.ShortIds;

public sealed class ShortIdGenerator(IOptions<ShortIdOptions> options) : IShortIdGenerator, ITransientDependency
{
    private const string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Numbers = "0123456789";
    private const string SpecialCharacters = "-_";

    public string Create()
    {
        var configuration = options.Value;
        var alphabet = Letters + (configuration.UseNumbers ? Numbers : string.Empty) + (configuration.UseSpecialCharacters ? SpecialCharacters : string.Empty);
        var length = configuration.Length > 0 ? configuration.Length : 8;
        var value = new StringBuilder(length);
        for (var index = 0; index < length; index++) value.Append(alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)]);
        return value.ToString();
    }

    public Task<string> CreateAsync() => Task.FromResult(Create());
}
