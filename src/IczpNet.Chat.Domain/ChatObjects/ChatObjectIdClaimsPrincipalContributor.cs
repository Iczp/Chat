//using System.Linq;
//using System.Security.Claims;
//using System.Security.Principal;
//using System.Threading.Tasks;
//using Volo.Abp.DependencyInjection;
//using Volo.Abp.Security.Claims;

//namespace IczpNet.Chat.ChatObjects;

//public class ChatObjectIdClaimsPrincipalContributor : IAbpClaimsPrincipalContributor, ITransientDependency
//{
//    protected IChatObjectManager ChatObjectManager { get; }

//    public ChatObjectIdClaimsPrincipalContributor(IChatObjectManager chatObjectManager)
//    {
//        ChatObjectManager = chatObjectManager;
//    }

//    public async Task ContributeAsync(AbpClaimsPrincipalContributorContext context)
//    {
//        var identity = context.ClaimsPrincipal.Identities.FirstOrDefault();

//        var userId = identity?.FindUserId();

//        if (!userId.HasValue)
//        {
//            return;
//        }

//        var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(userId.Value);

//        foreach (var chatObjectId in chatObjectIdList)
//        {
//            identity.AddClaim(new Claim(ChatObjectClaims.Id, chatObjectId.ToString()));
//        }

//        identity.AddIfNotContains(new Claim(ChatObjectClaims.Count, chatObjectIdList.Count.ToString(), ClaimValueTypes.Integer));
//    }
//}
