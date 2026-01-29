using Castle.Core.Internal;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Enums.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 枚举
/// </summary>
public class EnumAppService : ChatAppService, IEnumAppService
{
    // 使用 Lazy<T> 确保只初始化一次
    private static readonly Lazy<Dictionary<string, EnumTypeDto>> lazyEnumTypesMap = new(GenerateEnumTypes);

    protected static Dictionary<string, EnumTypeDto> EnumTypesMap => lazyEnumTypesMap.Value;

    private static Dictionary<string, EnumTypeDto> GenerateEnumTypes()
    {
        return typeof(ChatDomainSharedModule).Assembly.GetExportedTypes()
            .Where(x => x.IsEnum)
            .Select(x => new EnumTypeDto()
            {
                Id = x.FullName,
                Name = x.GetTypeAttribute<DescriptionAttribute>()?.Description,
                Items = [.. Enum.GetNames(x)
                   .Select(v => new EnumDto()
                   {
                       Name = v,
                       Value = (int)Enum.Parse(x, v),
                       Description = ((Enum)Enum.Parse(x, v)).GetDescription()
                   })]
            })
            .ToDictionary(x => x.Id);
    }

    /// <summary>
    /// 获取枚举列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public virtual async Task<PagedResultDto<EnumTypeDto>> GetListAsync(EnumGetListInput input)
    {
        var query = EnumTypesMap.Values.AsQueryable()
            .WhereIf(!input.Keyword.IsNullOrWhiteSpace(),
                x => (x.Name != null && x.Name.Contains(input.Keyword))
                    || x.Items.Any(v => v.Name.Contains(input.Keyword))
                    || x.Id == input.Keyword
            );

        return await GetPagedListAsync<EnumTypeDto, EnumTypeDto>(query, input);
    }

    /// <summary>
    /// 获取枚举内容
    /// </summary>
    /// <param name="id">枚举类型Fullname</param>
    /// <returns></returns>
    public virtual Task<EnumTypeDto> GetAsync(string id)
    {
        return Task.FromResult(EnumTypesMap.GetOrDefault(id));
    }
}
