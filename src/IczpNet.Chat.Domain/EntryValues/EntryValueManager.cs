using System;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.EntryValues;

public class EntryValueManager(IRepository<EntryValue, Guid> repository) : DomainService, IEntryValueManager
{
    protected IRepository<EntryValue, Guid> Repository { get; set; } = repository;
}
