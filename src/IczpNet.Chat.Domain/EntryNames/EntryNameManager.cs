using System;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.EntryNames
{
    public class EntryNameManager : DomainService, IEntryNameManager
    {
        protected IRepository<EntryName, Guid> Repository { get; set; }

        public EntryNameManager(
            IRepository<EntryName, Guid> repository)
        {
            Repository = repository;
        }
    }
}
