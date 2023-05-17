using Castle.Core.Internal;
using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Enums.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Enums
{
    public class EnumAppService : ChatAppService, IEnumAppService
    {
        protected static List<EnumTypeDto> EnumItems;

        public Task<PagedResultDto<EnumTypeDto>> GetAllAsync(EnumGetListInput input)
        {
            EnumItems ??= typeof(ChatDomainSharedModule).Assembly.GetExportedTypes()
                .Where(x => x.IsEnum)
                .Select(x => new EnumTypeDto()
                {
                    Type = x.FullName,
                    Description = x.GetTypeAttribute<DescriptionAttribute>()?.Description,
                    Names = Enum.GetNames(x)
                })
                .ToList();

            return Task.FromResult(new PagedResultDto<EnumTypeDto>(EnumItems.Count, EnumItems));
        }

        public Task<List<EnumDto>> GetListByTypeAsync(string type)
        {
            var _type = typeof(ChatDomainSharedModule).Assembly.GetType(type);

            Assert.NotNull(_type, $"Not found:{type}");

            var names = Enum.GetNames(_type);

            var result = names
               .Select(x => new EnumDto()
               {
                   Name = x,
                   Value = (int)Enum.Parse(_type, x),
                   Description = ((Enum)Enum.Parse(_type, x)).GetDescription()
               })
               .ToList();

            return Task.FromResult(result);
        }
    }
}
