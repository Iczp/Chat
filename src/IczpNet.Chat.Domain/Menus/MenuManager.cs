using IczpNet.AbpCommons;
using IczpNet.AbpTrees;
using IczpNet.BizCrypts;
using IczpNet.Chat.Developers;
using IczpNet.Chat.HttpRequests;
using Microsoft.Extensions.Logging;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Menus
{
    public class MenuManager : TreeManager<Menu, Guid, MenuInfo>, IMenuManager
    {
        protected IBackgroundJobManager BackgroundJobManager { get; }
        protected IHttpRequestManager HttpRequestManager { get; }

        public MenuManager(
            IRepository<Menu, Guid> repository,
            IBackgroundJobManager backgroundJobManager,
            IHttpRequestManager httpRequestManager) : base(repository)
        {
            BackgroundJobManager = backgroundJobManager;
            HttpRequestManager = httpRequestManager;
        }

        protected override async Task CheckExistsByCreateAsync(Menu inputEntity)
        {
            Assert.If(await Repository.AnyAsync(x => x.Name == inputEntity.Name && x.OwnerId == inputEntity.OwnerId), $"Already exists Name:{inputEntity.Name}");
        }

        protected override async Task CheckExistsByUpdateAsync(Menu inputEntity)
        {
            Assert.If(await Repository.AnyAsync((x) => x.Name == inputEntity.Name && x.OwnerId == inputEntity.OwnerId && !x.Id.Equals(inputEntity.Id)), $" Name[{inputEntity.Name}] already such");
        }

        /// <summary>
        /// 发送一个请求
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<HttpRequest> HttpGetRemoteHostAsync(Developer developer, string requestContent, string name = null)
        {
            var bizCrypt = new BizCrypt(developer.Token, developer.EncodingAesKey, developer.OwnerId.ToString());

            var timeStamp = DateTime.Now.Ticks.ToString();

            var nonce = BizCrypt.CreateRandCode(10, "123456789");

            var echo = bizCrypt.Encrypt(requestContent);

            var signature = BizCrypt.GenerateSignature(developer.Token, timeStamp, nonce, echo);

            var remoteUrl = ParseUrl(developer.PostUrl, new Dictionary<string, string> {
                {"signature",signature },
                {"timeStamp",timeStamp },
                {"nonce",nonce },
                {"echo",echo },
            });

            var responseResult = await HttpRequestManager.RequestAsync(HttpMethod.Get, remoteUrl, name: name);

            return responseResult;
        }

        /// <summary>
        /// ParseUrl
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected virtual string ParseUrl(string url, IDictionary<string, string> parameters)
        {
            var _uri = new Uri(url);

            var _params = parameters.Select(x => $"{x.Key}={HttpUtility.UrlEncode(x.Value)}").JoinAsString("&");

            var _query = new StringBuilder(_uri.Query);

            _query.Append(url.IndexOf('?') != -1 ? "&" : "?");

            _query.Append(_params);

            var retUrl = new string[] { _uri.Scheme, "://", _uri.Authority, _uri.AbsolutePath, _query.ToString(), _uri.Fragment };

            return retUrl.JoinAsString("");
        }


        public virtual async Task<string> TriggerAsync(Guid id)
        {
            //var menu = await Repository.GetAsync(id);

            //await CheckMenuAsync(menu);

            var jobId = await BackgroundJobManager.EnqueueAsync(new MenuTriggerArgs()
            {
                MenuId = id,
            });

            Logger.LogInformation($"Trigger:MenuId={id},jobId={jobId}");

            return jobId;
        }

        public virtual async Task CheckMenuAsync(Menu menu)
        {
            var owner = Assert.NotNull(menu.Owner, $"Owner is null");

            Assert.If(!owner.IsEnabled, $"IsEnabled:{owner.IsEnabled}");

            Assert.If(!owner.IsDeveloper, $"IsDeveloper:{owner.IsDeveloper}");

            var developer = Assert.NotNull(owner.Developer, $"Developer is null");

            Assert.If(!IsUrl(developer.PostUrl), $"Fail Url:{developer.PostUrl}", nameof(developer.PostUrl));

            await Task.Yield();
        }

        public virtual async Task<bool> IsCheckMenuAsync(Menu menu)
        {
            await Task.Yield();

            var owner = menu.Owner;

            return owner != null && owner.IsEnabled && owner.IsDeveloper && IsUrl(owner.Developer?.PostUrl);
        }

        private static bool IsUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            var httpSchemes = new string[] { Uri.UriSchemeHttp, Uri.UriSchemeHttps };

            return Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri uri) && httpSchemes.Contains(uri.Scheme);
        }

        public virtual async Task<HttpRequest> SendToRemoteHostAsync(Menu menu, string name = null)
        {
            //var menu = await Repository.GetAsync(id);

            //await CheckMenuAsync(menu);

            var req = await HttpGetRemoteHostAsync(menu.Owner.Developer, $"menuId:{menu}", name: name);

            Logger.LogInformation($"SendToRemoteHost ReqId={req.Id},[GET,{req.StatusCode}],url={req.Url}");

            return req;
        }
    }
}
