using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.BaseDtos;

public class ExtraPagedResultDto<T> : PagedResultDto<T>
{
    public ExtraPagedResultDto()
    {
    }

    public ExtraPagedResultDto(long totalCount, IReadOnlyList<T> items) : base(totalCount, items)
    {
    }

    public object Extra { get; set; }

}
