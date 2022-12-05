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
    }
}
