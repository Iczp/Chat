using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Connections;
using IczpNet.Chat.Connections.Dtos;
using IczpNet.Chat.MessageSections.Messages;
using Microsoft.AspNetCore.Mvc;
using Nito.AsyncEx;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.Services
{
    public class ConnectionAppService
        : CrudAppService<
            Connection,
            ConnectionDto,
            Guid,
            ConnectionGetListInput,
            ConnectionCreateInput>,
        IConnectionAppService
    {

        //protected IRepository<ChatObject, Guid> ChatObjectRepository { get; } 

        protected IChatObjectManager ChatObjectManager { get; }

        protected IConnectionManager ConnectionManager { get; }

        public ConnectionAppService(
            IRepository<Connection, Guid> repository,
            IChatObjectManager chatObjectManager,
            IConnectionManager connectionManager) : base(repository)
        {
            ChatObjectManager = chatObjectManager;
            ConnectionManager = connectionManager;
        }

        protected override async Task<IQueryable<Connection>> CreateFilteredQueryAsync(ConnectionGetListInput input)
        {



            return (await base.CreateFilteredQueryAsync(input))
                //.WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                //.WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                //.WhereIf(input.MinCount.HasValue, x => x.ConnectionMemberList.Count >= input.MinCount)
                //.WhereIf(input.MaxCount.HasValue, x => x.ConnectionMemberList.Count < input.MaxCount)
                //.WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                //.WhereIf(input.IsForbiddenAll.HasValue, x => x.IsForbiddenAll == input.IsForbiddenAll)
                //.WhereIf(input.MemberOwnerId.HasValue, x => x.ConnectionMemberList.Any(d => d.OwnerId == input.MemberOwnerId))
                //.WhereIf(input.ForbiddenMemberOwnerId.HasValue, x => x.ConnectionForbiddenMemberList.Any(d => d.OwnerId == input.ForbiddenMemberOwnerId && d.ExpireTime.HasValue && d.ExpireTime < DateTime.Now))
                //.WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Title.Contains(input.Keyword))

                ;
        }



        [HttpPost]
        public async Task<string> ActiveAsync(Guid id, string ticks)
        {
            await ConnectionManager.UpdateActiveTimeAsync(id);
            return ticks;
        }

        [RemoteService(false)]
        public override Task<ConnectionDto> UpdateAsync(Guid id, ConnectionCreateInput input)
        {
            return base.UpdateAsync(id, input);
        }

        [RemoteService(false)]
        public override Task DeleteAsync(Guid id)
        {
            return base.DeleteAsync(id);
        }

        [HttpGet]
        public async Task<GetOnlineCountOutput> GetOnlineCountAsync()
        {
            var currentTime = Clock.Now;
            var count = await ConnectionManager.GetOnlineCountAsync(currentTime);
            return new GetOnlineCountOutput { Count = count, CurrentTime = currentTime };
        }
    }
}
