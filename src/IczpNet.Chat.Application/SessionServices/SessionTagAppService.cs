using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Sessions;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionTags.Dtos;
using IczpNet.Chat.SessionTags;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionServices;

/// <summary>
/// 会话内标签
/// </summary>
public class SessionTagAppService
    : CrudChatAppService<
        SessionTag,
        SessionTagDetailDto,
        SessionTagDto,
        Guid,
        SessionTagGetListInput,
        SessionTagCreateInput,
        SessionTagUpdateInput>,
    ISessionTagAppService
{

    //protected override string GetPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionTagPermission.Default;
    //protected override string GetListPolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionTagPermission.Default;
    //protected override string CreatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionTagPermission.Create;
    //protected override string UpdatePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionTagPermission.Update;
    //protected override string DeletePolicyName { get; set; } = SessionPermissionDefinitionConsts.SessionTagPermission.Delete;

    protected IChatObjectRepository ChatObjectRepository { get; }
    protected IRepository<Session, Guid> SessionRepository { get; }
    public SessionTagAppService(
        IRepository<SessionTag, Guid> repository,
        IChatObjectRepository chatObjectRepository,
        IRepository<Session, Guid> sessionRepository) : base(repository)
    {
        ChatObjectRepository = chatObjectRepository;
        SessionRepository = sessionRepository;
    }

    protected override async Task<IQueryable<SessionTag>> CreateFilteredQueryAsync(SessionTagGetListInput input)
    {
        return await base.CreateFilteredQueryAsync(input)

            ;
    }

    protected override async Task<SessionTag> MapToEntityAsync(SessionTagCreateInput createInput)
    {
        //var owner = Assert.NotNull(await ChatObjectRepository.FindAsync(createInput.SessionId), $"No such Entity by SessionUnitId:{createInput.SessionId}.");

        Assert.If(await Repository.AnyAsync(x => x.SessionId == createInput.SessionId && x.Name == createInput.Name), $"Already exists [{createInput.Name}].");

        return new SessionTag(GuidGenerator.Create(), createInput.SessionId, createInput.Name);
    }

    /// <summary>
    /// 创建会话标签
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public override async Task<SessionTagDetailDto> CreateAsync(SessionTagCreateInput input)
    {
        Assert.If(!await SessionRepository.AnyAsync(x => x.Id == input.SessionId), $"No such entity of sessionId:{input.SessionId}");

        return await base.CreateAsync(input);
    }


    [RemoteService(false)]
    public override Task<SessionTagDetailDto> UpdateAsync(Guid id, SessionTagUpdateInput input)
    {
        return base.UpdateAsync(id, input);
    }
}
