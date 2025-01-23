using System;
using System.Threading.Tasks;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Blobs;

public class BlobManager(
    IBlobContainerFactory blobContainerFactory,
    IRepository<Blob, Guid> repository) : DomainService, IBlobManager
{
    protected IBlobContainerFactory BlobContainerFactory { get; set; } = blobContainerFactory;
    protected IRepository<Blob, Guid> Repository { get; set; } = repository;


    public virtual async Task<Blob> CreateAsync(Blob blob)
    {
        var entity = await Repository.InsertAsync(blob);
        await SaveBytesAsync(entity.Container, entity.Name, entity.Bytes);
        return entity;
    }

    public virtual async Task SaveBytesAsync(string container, string name, byte[] bytes)
    {
        var _blobContainer = BlobContainerFactory.Create(container);
        await _blobContainer.SaveAsync(name, bytes);
    }

    public virtual Task<byte[]> GetBytesAsync(string container, string name)
    {
        var _blobContainer = BlobContainerFactory.Create(container);
        return _blobContainer.GetAllBytesAsync(name);
    }

    public async Task<byte[]> GetBytesAsync(Guid id)
    {
        var blob = await Repository.GetAsync(id);
        return await GetBytesAsync(blob.Container, blob.Name);
    }

    public async Task<Blob> GetEntityAsync(string container, string name)
    {
        return await Repository.FindAsync(x => x.Container == container && x.Name == name);
    }

    public async Task<Blob> GetAsync(Guid id)
    {
        return await Repository.GetAsync(id);
    }

    public async Task<Blob> FindAsync(Guid id)
    {
        return await Repository.FindAsync(id);
    }


}
