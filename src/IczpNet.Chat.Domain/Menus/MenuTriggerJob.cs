using Castle.DynamicProxy;
using IczpNet.AbpCommons;
using IczpNet.BizCrypts;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Menus;

public class MenuTriggerJob(
    IMenuManager menuManager,
    IUnitOfWorkManager unitOfWorkManager,
    IRepository<Menu, Guid> repository,
    IMessageSender messageSender,
    ISessionUnitManager sessionUnitManager) : AsyncBackgroundJob<MenuTriggerArgs>, ITransientDependency
{
    protected IMenuManager MenuManager { get; } = menuManager;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;
    protected IRepository<Menu, Guid> Repository { get; } = repository;
    protected IMessageSender MessageSender { get; } = messageSender;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;

    [UnitOfWork]
    public override async Task ExecuteAsync(MenuTriggerArgs args)
    {
        using var uow = UnitOfWorkManager.Begin();

        var menu = await Repository.GetAsync(args.MenuId);

        if (!await MenuManager.IsCheckMenuAsync(menu))
        {
            Logger.LogWarning($@"MenuTriggerJob is not checked:MenuId={args.MenuId},IsEnabled={menu.Owner?.IsEnabled},IsDeveloper={menu.Owner?.IsDeveloper}");
            Logger.LogWarning($@"Developer.IsEnabled={menu.Owner?.Developer?.IsEnabled},Developer.PostUrl={menu.Owner?.Developer?.PostUrl},");
            return;
        }

        var typeName = ProxyUtil.GetUnproxiedType(this).FullName;

        var req = await MenuManager.SendToRemoteHostAsync(menu, args, name: typeName);

        Logger.LogInformation($"MenuTriggerJob HttpReqquestId={req.Id},[POST,{req.StatusCode}],url={req.Url}");

        if (req.IsSuccess && !string.IsNullOrWhiteSpace(req.Response?.Content) && args.SessionUnitId.HasValue)
        {
            var rawBody = req.Response.Content.Trim();
            if (!string.Equals(rawBody, "success", StringComparison.OrdinalIgnoreCase))
            {
                var developer = menu.Owner.Developer;
                var bizCrypt = new BizCrypt(developer.Token, developer.EncodingAesKey, developer.OwnerId.ToString());
                var replyText = TryExtractReplyText(rawBody, bizCrypt);
                if (!string.IsNullOrWhiteSpace(replyText))
                {
                    var senderSessionUnit = await SessionUnitManager.GetAsync(args.SessionUnitId.Value);
                    if (senderSessionUnit != null && senderSessionUnit.DestinationId.HasValue)
                    {
                        var officialSessionUnit = await SessionUnitManager.FindCacheAsync(senderSessionUnit.DestinationId.Value, senderSessionUnit.OwnerId);
                        if (officialSessionUnit != null)
                        {
                            await MessageSender.SendTextAsync(officialSessionUnit, new MessageInput<TextContentInfo>
                            {
                                Content = new TextContentInfo { Text = replyText }
                            });
                        }
                    }
                }
            }
        }

        uow.OnCompleted(async () =>
        {
            //throw: BackgroundJob retry
            Assert.If(!req.IsSuccess, $"Error:{req.Message},HttpReqquestId={req.Id},url={req.Url}");
            await Task.Yield();
        });
    }

    protected virtual string TryExtractReplyText(string rawBody, BizCrypt bizCrypt)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            if (doc.RootElement.TryGetProperty("encrypt", out var encryptProp))
            {
                var cipher = encryptProp.GetString();
                if (!string.IsNullOrWhiteSpace(cipher))
                {
                    var decrypted = bizCrypt.Decrypt(cipher);
                    return ExtractFromContentOrRaw(decrypted);
                }
            }
            else if (doc.RootElement.TryGetProperty("Content", out var contentProp) ||
                     doc.RootElement.TryGetProperty("content", out contentProp))
            {
                return contentProp.GetString();
            }
        }
        catch
        {
            try
            {
                var decrypted = bizCrypt.Decrypt(rawBody);
                return ExtractFromContentOrRaw(decrypted);
            }
            catch
            {
                return rawBody;
            }
        }
        return rawBody;
    }

    private static string ExtractFromContentOrRaw(string decrypted)
    {
        if (string.IsNullOrWhiteSpace(decrypted)) return null;
        try
        {
            using var doc = JsonDocument.Parse(decrypted);
            if (doc.RootElement.TryGetProperty("Content", out var contentProp) ||
                doc.RootElement.TryGetProperty("content", out contentProp))
            {
                return contentProp.GetString();
            }
        }
        catch { }
        return decrypted;
    }
}
