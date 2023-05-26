using Volo.Abp.DependencyInjection;
using Volo.Abp.ObjectMapping;
using IczpNet.Chat.MessageSections.Templates;
using Volo.Abp.Domain.Services;
using Microsoft.Extensions.Logging;

namespace IczpNet.Chat.MessageSections.Mappers
{
    public class TextContentInfoMapper : DomainService, IObjectMapper<TextContentInfo, TextContent>, ITransientDependency
    {
        protected ITextFilter TextFilter { get; }

        public TextContentInfoMapper(ITextFilter textFilter)
        {
            TextFilter = textFilter;
        }

        public TextContent Map(TextContentInfo source)
        {
            Logger.LogInformation($"TextContentInfo:{source}");

            TextFilter.CheckAsync(source.Text).Wait();

            return new TextContent(GuidGenerator.Create())
            {
                Text = source.Text,
            };
        }

        public TextContent Map(TextContentInfo source, TextContent destination)
        {
            TextFilter.CheckAsync(source.Text).Wait();

            Logger.LogInformation($"TextContentInfo:{source},TextContent:{destination}");

            return destination;
        }
    }
}
