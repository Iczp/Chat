using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Messages.Dtos;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.Messages;

public interface IMessageAdminAppService : IApplicationService
{
    Task<MessageAdminHistoryResultDto> GetHistoryAsync(
        MessageAdminGetHistoryInput input,
        CancellationToken cancellationToken = default);

    Task<MessageInfo<ArticleContentInfo>> SendArticleContentMessageAsync(
        Guid sessionUnitId,
        MessageInput<ArticleContentInput> input,
        CancellationToken cancellationToken = default);

    Task<MessageInfo<TemplateContentInfo>> SendTemplateContentMessageAsync(
        Guid sessionUnitId,
        MessageInput<TemplateContentInput> input,
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, long>> RollbackAsync(
        long messageId,
        CancellationToken cancellationToken = default);
}
