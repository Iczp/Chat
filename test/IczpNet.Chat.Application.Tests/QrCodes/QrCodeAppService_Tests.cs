using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.QrCodes.Dtos;
using IczpNet.Chat.ScanCodes;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Claims;
using Xunit;

namespace IczpNet.Chat.QrCodes;

public class QrCodeAppService_Tests : ChatApplicationTestBase
{
    private static readonly Guid DefaultUserId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");

    private readonly IQrCodeAppService _qrCodeAppService;
    private readonly IScanCodeAppService _scanCodeAppService;
    private readonly IRepository<ChatObject, long> _chatObjectRepository;
    private readonly IRepository<ChatObjectType, string> _chatObjectTypeRepository;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    protected override void AfterAddApplication(IServiceCollection services)
    {
        var multiplexer = Substitute.For<IConnectionMultiplexer>();
        var database = Substitute.For<IDatabase>();
        multiplexer.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);
        services.AddSingleton(multiplexer);
        services.AddSingleton(Substitute.For<IBackgroundJobManager>());
    }

    public QrCodeAppService_Tests()
    {
        ServiceProvider.UseStaticObjectMapper();
        _qrCodeAppService = GetRequiredService<IQrCodeAppService>();
        _scanCodeAppService = GetRequiredService<IScanCodeAppService>();
        _chatObjectRepository = GetRequiredService<IRepository<ChatObject, long>>();
        _chatObjectTypeRepository = GetRequiredService<IRepository<ChatObjectType, string>>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private readonly System.Collections.Generic.Dictionary<ChatObjectTypeEnums, ChatObjectType> _chatObjectTypes = new();

    private async Task EnsureChatObjectTypesAsync()
    {
        foreach (var enumVal in new[]
        {
            ChatObjectTypeEnums.Personal,
            ChatObjectTypeEnums.ShopKeeper
        })
        {
            var id = enumVal.ToString();
            var entity = await _chatObjectTypeRepository.FindAsync(id);
            if (entity == null)
            {
                entity = await _chatObjectTypeRepository.InsertAsync(new ChatObjectType(id)
                {
                    Name = id,
                    ObjectType = enumVal
                }, autoSave: true);
            }
            _chatObjectTypes[enumVal] = entity;
        }
    }

    private async Task<ChatObject> CreateChatObjectAsync(
        string name,
        ChatObjectTypeEnums type,
        Guid appUserId,
        VerificationMethods verificationMethod = VerificationMethods.None)
    {
        var entity = new ChatObject(name, $"{type}_{Guid.NewGuid():N}"[..16], _chatObjectTypes[type], null);
        entity.SetAppUserId(appUserId);
        entity.SetVerificationMethod(verificationMethod);
        return await _chatObjectRepository.InsertAsync(entity, autoSave: true);
    }

    [Fact]
    public async Task Generate_Get_Resolve_Reset_Apply_And_Scan_Should_Work()
    {
        long ownerId = 0;
        long shopId = 0;
        long viewerId = 0;
        Guid viewerUserId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await EnsureChatObjectTypesAsync();
            var owner = await CreateChatObjectAsync("AppOwner", ChatObjectTypeEnums.Personal, DefaultUserId);
            var shop = await CreateChatObjectAsync("AppShop", ChatObjectTypeEnums.ShopKeeper, DefaultUserId);
            var viewer = await CreateChatObjectAsync("AppViewer", ChatObjectTypeEnums.Personal, viewerUserId);
            ownerId = owner.Id;
            shopId = shop.Id;
            viewerId = viewer.Id;
        });

        // 1. 生成名片二维码 + 自定义天数
        var generated = await _qrCodeAppService.GenerateAsync(new QrCodeGenerateInput
        {
            OwnerId = shopId,
            IsPermanent = false,
            ExpireDays = 30,
            Title = "店铺名片"
        });
        generated.OwnerId.ShouldBe(shopId);
        generated.IsPermanent.ShouldBeFalse();
        generated.ExpireDays.ShouldBe(30);
        generated.Content.ShouldBe($"gotoim://qr/{generated.Code}");

        // 2. GetAsync 详情映射与本人权限放行
        var detail = await _qrCodeAppService.GetAsync(generated.Id);
        detail.Id.ShouldBe(generated.Id);
        detail.OwnerName.ShouldBe("AppShop");
        detail.IsExpired.ShouldBeFalse();

        // 3. GetByCodeAsync & ResolveAsync
        var byCode = await _qrCodeAppService.GetByCodeAsync(generated.Code, viewerId);
        byCode.IsValid.ShouldBeTrue();
        byCode.OwnerId.ShouldBe(shopId);
        byCode.IsSelf.ShouldBeFalse();

        var resolvedByOtherRole = await _qrCodeAppService.ResolveAsync(generated.Content, ownerId);
        resolvedByOtherRole.IsValid.ShouldBeTrue();
        resolvedByOtherRole.IsSelf.ShouldBeFalse();

        var resolvedSelf = await _qrCodeAppService.ResolveAsync(generated.Content, shopId);
        resolvedSelf.IsValid.ShouldBeTrue();
        resolvedSelf.IsSelf.ShouldBeTrue();

        // 4. ScanCodeAppService.ScanAsync 识别
        var scanResult = await _scanCodeAppService.ScanAsync("QR_CODE", generated.Content, viewerId);
        scanResult.ScanHandlers.ShouldContain(x => x.Success && x.Action == "chat_object_card");

        // 5. ResetAsync 重置旧码后旧码失效
        var reset = await _qrCodeAppService.ResetAsync(new QrCodeGenerateInput
        {
            OwnerId = shopId,
            IsPermanent = true
        });
        reset.Code.ShouldNotBe(generated.Code);
        reset.IsPermanent.ShouldBeTrue();

        var oldCheck = await _qrCodeAppService.GetByCodeAsync(generated.Code, viewerId);
        oldCheck.IsValid.ShouldBeFalse();
        oldCheck.IsDisabled.ShouldBeTrue();

        // 6. 切换为 viewer 用户执行 ApplyAsync 加好友
        var viewerPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(AbpClaimTypes.UserId, viewerUserId.ToString())
        }));
        using (_currentPrincipalAccessor.Change(viewerPrincipal))
        {
            var applyOutput = await _qrCodeAppService.ApplyAsync(new QrCodeApplyInput
            {
                Code = reset.Content,
                ApplicantChatObjectId = viewerId
            });
            applyOutput.Status.ShouldBe("added");
            applyOutput.TargetChatObjectId.ShouldBe(shopId);
            applyOutput.SessionUnitId.ShouldNotBeNull();
        }

        // 7. 通用自定义场景（工作台应用分享）
        var workbenchQr = await _qrCodeAppService.GenerateAsync(new QrCodeGenerateInput
        {
            Scene = QrCodeScenes.WorkbenchApp,
            InviterId = ownerId,
            ExpireDays = 7,
            Title = "ERP工单应用",
            PayloadJson = "{\"appId\":\"erp-ticket\",\"path\":\"/pages/ticket/detail?id=42\"}"
        });
        workbenchQr.Scene.ShouldBe(QrCodeScenes.WorkbenchApp);
        workbenchQr.ExpireDays.ShouldBe(7);

        var workbenchScan = await _qrCodeAppService.ResolveAsync(workbenchQr.Content, viewerId);
        workbenchScan.IsValid.ShouldBeTrue();
        workbenchScan.Action.ShouldBe("workbench_app");
        workbenchScan.PayloadJson.ShouldContain("erp-ticket");
    }
}
