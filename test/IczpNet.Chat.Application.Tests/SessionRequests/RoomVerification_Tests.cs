using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.QrCodes;
using IczpNet.Chat.QrCodes.Dtos;
using IczpNet.Chat.RoomSections.Rooms;
using IczpNet.Chat.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionRequests;
using IczpNet.Chat.SessionSections.SessionRequests.Dtos;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.Authorization;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Claims;
using Xunit;

namespace IczpNet.Chat.SessionRequests;

public class RoomVerification_Tests : ChatApplicationTestBase
{
    private static readonly Guid DefaultUserId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");

    private readonly IRoomAppService _roomAppService;
    private readonly IRoomManager _roomManager;
    private readonly IQrCodeAppService _qrCodeAppService;
    private readonly ISessionRequestAppService _sessionRequestAppService;
    private readonly ISessionUnitRepository _sessionUnitRepository;
    private readonly IRepository<ChatObject, long> _chatObjectRepository;
    private readonly IRepository<ChatObjectType, string> _chatObjectTypeRepository;
    private readonly IRepository<SessionPermissionDefinition, string> _permDefRepo;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    protected override void AfterAddApplication(IServiceCollection services)
    {
        var multiplexer = Substitute.For<IConnectionMultiplexer>();
        var database = Substitute.For<IDatabase>();
        multiplexer.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(database);
        services.AddSingleton(multiplexer);
        services.AddSingleton(Substitute.For<IBackgroundJobManager>());
    }

    public RoomVerification_Tests()
    {
        ServiceProvider.UseStaticObjectMapper();
        _roomAppService = GetRequiredService<IRoomAppService>();
        _roomManager = GetRequiredService<IRoomManager>();
        _qrCodeAppService = GetRequiredService<IQrCodeAppService>();
        _sessionRequestAppService = GetRequiredService<ISessionRequestAppService>();
        _sessionUnitRepository = GetRequiredService<ISessionUnitRepository>();
        _chatObjectRepository = GetRequiredService<IRepository<ChatObject, long>>();
        _chatObjectTypeRepository = GetRequiredService<IRepository<ChatObjectType, string>>();
        _permDefRepo = GetRequiredService<IRepository<SessionPermissionDefinition, string>>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private async Task EnsureSeedAsync()
    {
        foreach (var enumVal in new[] { ChatObjectTypeEnums.Personal, ChatObjectTypeEnums.Room })
        {
            var id = enumVal.ToString();
            if (await _chatObjectTypeRepository.FindAsync(id) == null)
            {
                await _chatObjectTypeRepository.InsertAsync(new ChatObjectType(id)
                {
                    Name = id,
                    ObjectType = enumVal
                }, autoSave: true);
            }
        }

        var handlePermId = SessionPermissionDefinitionConsts.SessionRequestPermission.Handle;
        if (await _permDefRepo.FindAsync(handlePermId) == null)
        {
            await _permDefRepo.InsertAsync(new SessionPermissionDefinition(handlePermId)
            {
                Name = "HandleSessionRequest",
                IsEnabled = true
            }, autoSave: true);
        }
    }

    private async Task<ChatObject> CreatePersonalAsync(string name, Guid userId)
    {
        var type = await _chatObjectTypeRepository.GetAsync(ChatObjectTypeEnums.Personal.ToString());
        var obj = new ChatObject(name, $"U_{Guid.NewGuid():N}"[..12], type, null);
        obj.SetAppUserId(userId);
        return await _chatObjectRepository.InsertAsync(obj, autoSave: true);
    }

    [Fact]
    public async Task Room_VerificationMethod_And_Admin_Approval_Workflow_Should_Work()
    {
        long creatorId = 0;
        long memberId = 0;
        long applicant1Id = 0;
        long applicant2Id = 0;
        Guid memberUserId = Guid.NewGuid();
        Guid applicant1UserId = Guid.NewGuid();
        Guid applicant2UserId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            await EnsureSeedAsync();
            creatorId = (await CreatePersonalAsync("Creator", DefaultUserId)).Id;
            memberId = (await CreatePersonalAsync("Member", memberUserId)).Id;
            applicant1Id = (await CreatePersonalAsync("Applicant1", applicant1UserId)).Id;
            applicant2Id = (await CreatePersonalAsync("Applicant2", applicant2UserId)).Id;
        });

        var room = await _roomManager.CreateAsync("VerifyRoom", new List<long> { memberId }, creatorId);

        Guid creatorUnitId = Guid.Empty;
        Guid memberUnitId = Guid.Empty;
        await WithUnitOfWorkAsync(async () =>
        {
            creatorUnitId = (await _sessionUnitRepository.FindAsync(x => x.OwnerId == creatorId && x.DestinationId == room.Id))!.Id;
            memberUnitId = (await _sessionUnitRepository.FindAsync(x => x.OwnerId == memberId && x.DestinationId == room.Id))!.Id;
        });

        // 1. 群主设置加群需要验证
        var updatedRoom = await _roomAppService.SetVerificationMethodAsync(creatorUnitId, VerificationMethods.Required);
        updatedRoom.VerificationMethod.ShouldBe(VerificationMethods.Required);

        // 普通群成员无权修改验证方式
        var memberPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(AbpClaimTypes.UserId, memberUserId.ToString()) }));
        using (_currentPrincipalAccessor.Change(memberPrincipal))
        {
            await Should.ThrowAsync<Exception>(() =>
                _roomAppService.SetVerificationMethodAsync(memberUnitId, VerificationMethods.None));
        }

        // 2. 生成群二维码，两个申请人扫码入群 -> 产生两条待审批 SessionRequest
        var qr = await _qrCodeAppService.GenerateAsync(new QrCodeGenerateInput
        {
            OwnerId = room.Id,
            InviterId = creatorId
        });

        Guid req1Id = Guid.Empty;
        Guid req2Id = Guid.Empty;
        using (_currentPrincipalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(AbpClaimTypes.UserId, applicant1UserId.ToString()) }))))
        {
            var res1 = await _qrCodeAppService.ApplyAsync(new QrCodeApplyInput { Code = qr.Code, ApplicantChatObjectId = applicant1Id, RequestMessage = "请通过1" });
            res1.Status.ShouldBe("request_sent");
            req1Id = res1.SessionRequestId!.Value;
        }
        using (_currentPrincipalAccessor.Change(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(AbpClaimTypes.UserId, applicant2UserId.ToString()) }))))
        {
            var res2 = await _qrCodeAppService.ApplyAsync(new QrCodeApplyInput { Code = qr.Code, ApplicantChatObjectId = applicant2Id, RequestMessage = "请通过2" });
            res2.Status.ShouldBe("request_sent");
            req2Id = res2.SessionRequestId!.Value;
        }

        // 3. 普通群成员无权审批入群申请
        using (_currentPrincipalAccessor.Change(memberPrincipal))
        {
            await Should.ThrowAsync<AbpAuthorizationException>(() =>
                _sessionRequestAppService.HandleRequestAsync(new SessionRequestHandleInput
                {
                    SessionRequestId = req1Id,
                    IsAgreed = true
                }));
        }

        // 4. 群主可查询群申请列表、读取详情、同意 req1、拒绝 req2
        var list = await _sessionRequestAppService.GetListAsync(new SessionRequestGetListInput { DestinationId = room.Id });
        list.TotalCount.ShouldBeGreaterThanOrEqualTo(2);

        var detail1 = await _sessionRequestAppService.GetAsync(req1Id);
        detail1.RequestMessage.ShouldBe("请通过1");

        var agreed = await _sessionRequestAppService.HandleRequestAsync(new SessionRequestHandleInput
        {
            SessionRequestId = req1Id,
            IsAgreed = true,
            HandleMessage = "欢迎加入"
        });
        agreed.IsHandled.ShouldBeTrue();
        agreed.IsAgreed.ShouldBe(true);

        var declined = await _sessionRequestAppService.HandleRequestAsync(new SessionRequestHandleInput
        {
            SessionRequestId = req2Id,
            IsAgreed = false,
            HandleMessage = "暂不符合入群条件"
        });
        declined.IsHandled.ShouldBeTrue();
        declined.IsAgreed.ShouldBe(false);

        await WithUnitOfWorkAsync(async () =>
        {
            (await _sessionUnitRepository.FindAsync(x => x.OwnerId == applicant1Id && x.DestinationId == room.Id)).ShouldNotBeNull();
            (await _sessionUnitRepository.FindAsync(x => x.OwnerId == applicant2Id && x.DestinationId == room.Id)).ShouldBeNull();
        });
    }
}
