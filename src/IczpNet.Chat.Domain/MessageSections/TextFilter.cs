using IczpNet.AbpCommons;
using NUglify.Helpers;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageSections
{
    public class TextFilter : DomainService, ITextFilter
    {
        public virtual async Task CheckAsync(string text)
        {
            var inputString = @"";

            var patterns = inputString.Split("\r\n")
                .Where(x => !x.IsNullOrWhiteSpace())
                .Select(x => x.Trim())
                ;

            foreach (var pattern in patterns)
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                var matches = regex.Matches(text);
                var match = regex.Match(text);
                Assert.If(regex.IsMatch(text), $"不能发送'{match.Value}'", "Gag");
            }

            await Task.CompletedTask;
        }

        public virtual async Task<bool> IsContainsAsync(string text)
        {
            await Task.CompletedTask;
            return false;
        }
    }
}
