using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.RobotSections.Robots;
using IczpNet.Chat.RobotSections.Robots.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services
{
    public class RobotAppService
        : CrudChatAppService<
            Robot,
            RobotDetailDto,
            RobotDto,
            Guid,
            RobotGetListInput,
            RobotCreateInput,
            RobotUpdateInput>,
        IRobotAppService
    {
        public RobotAppService(IRepository<Robot, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<Robot>> CreateFilteredQueryAsync(RobotGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword) || x.Description.Contains(input.Keyword))

                ;
        }

        protected override Robot MapToEntity(RobotCreateInput createInput)
        {
            var entity = base.MapToEntity(createInput);
            entity.SetName(createInput.Name);
            return entity;
        }

        protected override async Task CheckCreateAsync(RobotCreateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => x.Name.Equals(input.Name)), $"Already exists [{typeof(Robot)}] name:{input.Name}");
            await base.CheckCreateAsync(input);
        }

        protected override async Task CheckUpdateAsync(Guid id, Robot entity, RobotUpdateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => x.Id.Equals(id) && x.Name.Equals(input.Name)), $"Already exists [{typeof(Robot)}] name:{input.Name}");
            await base.CheckUpdateAsync(id, entity, input);
        }

       
    }
}
