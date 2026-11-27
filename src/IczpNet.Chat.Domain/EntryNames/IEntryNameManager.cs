using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.AbpTrees;

namespace IczpNet.Chat.EntryNames;

public class EntryNameAdminListResult
{
    public long TotalCount { get; set; }

    public List<EntryName> Items { get; set; } = new();
}

public class EntryNameAdminInputModel
{
    public string Name { get; set; }

    public Guid? ParentId { get; set; }

    public string Code { get; set; }

    public string InputType { get; set; }

    public string Help { get; set; }

    public string DefaultValue { get; set; }

    public string Regex { get; set; }

    public int? MaxLength { get; set; }

    public int? MinLength { get; set; }

    public int? MaxCount { get; set; }

    public int? MinCount { get; set; }

    public bool IsChoice { get; set; }

    public bool IsUniqued { get; set; }

    public bool IsRequired { get; set; }

    public bool IsPublic { get; set; } = true;

    public string ErrorMessage { get; set; }

    public double Sorting { get; set; }

    public string Description { get; set; }
}

public interface IEntryNameManager : ITreeManager<EntryName, Guid, TreeInfo<Guid>>
{
    Task<EntryNameAdminListResult> GetAdminListAsync(
        string keyword = null,
        Guid? parentId = null,
        bool filterByParentId = false,
        int skipCount = 0,
        int maxResultCount = 200,
        string sorting = null,
        CancellationToken cancellationToken = default);

    Task<EntryName> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<EntryName> CreateAdminAsync(
        EntryNameAdminInputModel input,
        CancellationToken cancellationToken = default);

    Task<EntryName> UpdateAdminAsync(
        Guid id,
        EntryNameAdminInputModel input,
        CancellationToken cancellationToken = default);

    Task DeleteAdminAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
