using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.RobotSections.Robots.Dtos;
using IczpNet.Chat.RobotSections.Robots;
using IczpNet.Chat.SquareSections.SquareCategorys;
using IczpNet.Chat.SquareSections.Squares;
using IczpNet.Chat.SquareSections.Squares.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using IczpNet.AbpCommons;

namespace IczpNet.Chat.SquareServices
{
    public class SquareAppService
        : CrudChatAppService<
            Square,
            SquareDetailDto,
            SquareDto,
            Guid,
            SquareGetListInput,
            SquareCreateInput,
            SquareUpdateInput>,
        ISquareAppService
    {

        protected ISquareCategoryManager SquareCategoryManager { get; }

        public SquareAppService(IRepository<Square, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<Square>> CreateFilteredQueryAsync(SquareGetListInput input)
        {
            //Category
            IQueryable<Guid> categoryIdQuery = null;

            if (input.IsImportChildCategory && input.CategoryIdList.IsAny())
            {
                categoryIdQuery = (await SquareCategoryManager.QueryCurrentAndAllChildsAsync(input.CategoryIdList)).Select(x => x.Id);
            }

            return (await ReadOnlyRepository.GetQueryableAsync())
                //CategoryId
                .WhereIf(!input.IsImportChildCategory && input.CategoryIdList.IsAny(), x => input.CategoryIdList.Contains(x.CategoryId.Value))
                .WhereIf(input.IsImportChildCategory && input.CategoryIdList.IsAny(), x => categoryIdQuery.Contains(x.CategoryId.Value))
                .WhereIf(!input.Keyword.IsNullOrEmpty(), x => x.Name.Contains(input.Keyword) || x.Code.Contains(input.Keyword));
        }

        protected override Square MapToEntity(SquareCreateInput createInput)
        {
            var entity = base.MapToEntity(createInput);
            entity.SetName(createInput.Name);
            return entity;
        }

        protected override async Task CheckCreateAsync(SquareCreateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => x.Name.Equals(input.Name)), $"Already exists [{typeof(Square)}] name:{input.Name}");
            await base.CheckCreateAsync(input);
        }

        protected override async Task CheckUpdateAsync(Guid id, Square entity, SquareUpdateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => x.Id.Equals(id) && x.Name.Equals(input.Name)), $"Already exists [{typeof(Square)}] name:{input.Name}");
            await base.CheckUpdateAsync(id, entity, input);
        }
    }
}
