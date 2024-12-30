using System;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.EntryValues;

public class EntryValueManager : DomainService, IEntryValueManager
{
    protected IRepository<EntryValue, Guid> Repository { get; set; }

    public EntryValueManager(IRepository<EntryValue, Guid> repository)
    {
        Repository = repository;
    }
}
