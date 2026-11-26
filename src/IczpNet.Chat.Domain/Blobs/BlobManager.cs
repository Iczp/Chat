using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Blobs;

public class BlobManager(
    IBlobContainerFactory blobContainerFactory,
    IRepository<Blob, Guid> repository,
    IRepository<BlobObject, Guid> blobObjectRepository,
    IAbpDistributedLock distributedLock) : DomainService, IBlobManager
{
    protected IBlobContainerFactory BlobContainerFactory { get; set; } = blobContainerFactory;
    protected IRepository<Blob, Guid> Repository { get; set; } = repository;
    protected IRepository<BlobObject, Guid> BlobObjectRepository { get; set; } = blobObjectRepository;

    public virtual async Task<Blob> CreateAsync(Blob blob)
    {
        if (blob.StorageObjectId.HasValue)
        {
            var sharedObj = await BlobObjectRepository.FindAsync(blob.StorageObjectId.Value);
            if (sharedObj == null || sharedObj.Status != BlobObjectStatus.Ready)
            {
                throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
            }

            var reuseLockKey = BuildObjectLockName(sharedObj.StorageScope, sharedObj.Container, sharedObj.Sha256, sharedObj.FileSize);
            await using var reuseHandle = await distributedLock.TryAcquireAsync(reuseLockKey, TimeSpan.FromSeconds(15));
            if (reuseHandle == null)
            {
                throw new UserFriendlyException("Blob storage object is busy, please retry.");
            }

            sharedObj = await BlobObjectRepository.GetAsync(blob.StorageObjectId.Value);
            if (sharedObj.Status != BlobObjectStatus.Ready)
            {
                throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
            }

            sharedObj.RefCount++;
            await BlobObjectRepository.UpdateAsync(sharedObj, autoSave: true);
            blob.SetStorageObject(sharedObj);
            return await Repository.InsertAsync(blob, autoSave: true);
        }

        if (blob.Bytes == null || blob.Bytes.Length == 0)
        {
            // Attachment content can be saved in a later application-service UOW.
            // Flush metadata first so its BlobId FK exists before the message content insert.
            var entity = await Repository.InsertAsync(blob, autoSave: true);
            if (entity.Bytes != null)
            {
                await SaveBytesAsync(entity.Container, entity.Name, entity.Bytes);
            }
            return entity;
        }

        var sha256 = Convert.ToHexString(SHA256.HashData(blob.Bytes)).ToLowerInvariant();
        var fileSize = blob.Bytes.LongLength;
        var storageScope = CurrentTenant.Id?.ToString("N") ?? "host";
        var lockName = BuildObjectLockName(storageScope, blob.Container, sha256, fileSize);

        await using var handle = await distributedLock.TryAcquireAsync(lockName, TimeSpan.FromSeconds(15));
        if (handle == null)
        {
            throw new UserFriendlyException("Blob storage object is busy, please retry.");
        }

        var existing = await BlobObjectRepository.FindAsync(x =>
            x.StorageScope == storageScope &&
            x.Container == blob.Container &&
            x.Sha256 == sha256 &&
            x.FileSize == fileSize);

        if (existing != null && existing.Status == BlobObjectStatus.Ready)
        {
            existing.RefCount++;
            await BlobObjectRepository.UpdateAsync(existing, autoSave: true);
            blob.SetStorageObject(existing);
            return await Repository.InsertAsync(blob, autoSave: true);
        }

        if (existing != null)
        {
            if (existing.Status == BlobObjectStatus.Deleting && !string.IsNullOrWhiteSpace(existing.StorageName))
            {
                try
                {
                    await BlobContainerFactory.Create(existing.Container).DeleteAsync(existing.StorageName);
                }
                catch
                {
                    // Best-effort removal of superseded object key before writing a new versioned key.
                }
            }

            existing.StorageName = BuildVersionedStorageName(sha256, GuidGenerator.Create(), blob.Suffix);
            existing.Status = BlobObjectStatus.Pending;
            existing.DeletingSince = null;
            existing.MimeType = blob.MimeType;
            await BlobObjectRepository.UpdateAsync(existing, autoSave: true);

            await SaveBytesAsync(existing.Container, existing.StorageName, blob.Bytes);

            existing.Status = BlobObjectStatus.Ready;
            existing.RefCount = Math.Max(0, existing.RefCount) + 1;
            existing.LastVerifiedTime = Clock.Now;
            await BlobObjectRepository.UpdateAsync(existing, autoSave: true);

            blob.SetStorageObject(existing);
            return await Repository.InsertAsync(blob, autoSave: true);
        }

        var objId = GuidGenerator.Create();
        var storageName = BuildVersionedStorageName(sha256, objId, blob.Suffix);
        var blobObject = new BlobObject(objId, storageScope, blob.Container, storageName, sha256, fileSize, blob.MimeType);
        blobObject = await BlobObjectRepository.InsertAsync(blobObject, autoSave: true);

        await SaveBytesAsync(blobObject.Container, blobObject.StorageName, blob.Bytes);

        blobObject.Status = BlobObjectStatus.Ready;
        blobObject.RefCount = 1;
        blobObject.LastVerifiedTime = Clock.Now;
        await BlobObjectRepository.UpdateAsync(blobObject, autoSave: true);

        blob.SetStorageObject(blobObject);
        return await Repository.InsertAsync(blob, autoSave: true);
    }

    public virtual async Task<(string Container, string StorageName)> ResolvePhysicalLocationAsync(
        Blob blob,
        CancellationToken cancellationToken = default)
    {
        if (blob == null)
        {
            return (null, null);
        }

        if (blob.StorageObject != null)
        {
            return (blob.StorageObject.Container, blob.StorageObject.StorageName);
        }

        if (blob.StorageObjectId.HasValue)
        {
            var storageObject = await BlobObjectRepository.FindAsync(blob.StorageObjectId.Value, cancellationToken: cancellationToken);
            if (storageObject != null)
            {
                blob.SetStorageObject(storageObject);
                return (storageObject.Container, storageObject.StorageName);
            }
        }

        return (blob.Container, blob.Name);
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
        var (container, storageName) = await ResolvePhysicalLocationAsync(blob);
        return await GetBytesAsync(container, storageName);
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

    private static string BuildObjectLockName(string storageScope, string container, string sha256, long fileSize)
        => $"Chat:BlobObject:{storageScope}:{container}:{sha256}:{fileSize}";

    private static string BuildVersionedStorageName(string sha256, Guid versionId, string suffix)
    {
        var cleanSuffix = string.IsNullOrWhiteSpace(suffix)
            ? string.Empty
            : (suffix.StartsWith('.') ? suffix.ToLowerInvariant() : $".{suffix.ToLowerInvariant()}");
        return $"objects/{sha256[..2]}/{sha256[2..4]}/{sha256}_{versionId:N}{cleanSuffix}";
    }
}
