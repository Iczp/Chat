using IczpNet.Chat.Options;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

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

    private bool IsPhysicalDeduplicationEnabled()
    {
        var opts = LazyServiceProvider?.LazyGetService<IOptions<AttachmentUploadOptions>>()?.Value;
        return opts?.EnablePhysicalDeduplication ?? true;
    }

    private Task RunCommittedStepAsync(Func<Task> action)
    {
        var uowManager = LazyServiceProvider?.LazyGetService<IUnitOfWorkManager>();
        if (uowManager == null)
        {
            return action();
        }

        return ExecuteInNewUowAsync(uowManager, action);
    }

    private async Task<T> RunCommittedStepAsync<T>(Func<Task<T>> action)
    {
        var uowManager = LazyServiceProvider?.LazyGetService<IUnitOfWorkManager>();
        if (uowManager == null)
        {
            return await action();
        }

        using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);
        var result = await action();
        await uow.CompleteAsync();
        return result;
    }

    private static async Task ExecuteInNewUowAsync(IUnitOfWorkManager uowManager, Func<Task> action)
    {
        using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);
        await action();
        await uow.CompleteAsync();
    }

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

            var verifiedObj = await RunCommittedStepAsync(async () =>
            {
                var current = await BlobObjectRepository.FindAsync(blob.StorageObjectId.Value);
                if (current == null || current.Status != BlobObjectStatus.Ready)
                {
                    throw new BusinessException(ChatErrorCodes.Blobs.AccessDenied);
                }

                current.RefCount++;
                return await BlobObjectRepository.UpdateAsync(current, autoSave: true);
            });

            blob.SetStorageObject(null);
            blob.StorageObjectId = verifiedObj.Id;
            return await Repository.InsertAsync(blob, autoSave: true);
        }

        if (!IsPhysicalDeduplicationEnabled() || blob.Bytes == null || blob.Bytes.Length == 0)
        {
            // Legacy non-dedup write or empty bytes: persist logical record and physical bytes directly.
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

        // Phase 1 (committed before any physical I/O): claim existing Ready object or allocate Pending record.
        Guid pendingOrReadyId = Guid.Empty;
        string containerToWrite = blob.Container;
        string storageNameToWrite = null;
        string oldDeletingKey = null;
        BlobObject readyObject = null;

        await RunCommittedStepAsync(async () =>
        {
            var existing = await BlobObjectRepository.FindAsync(x =>
                x.StorageScope == storageScope &&
                x.Container == blob.Container &&
                x.Sha256 == sha256 &&
                x.FileSize == fileSize);

            if (existing != null && existing.Status == BlobObjectStatus.Ready)
            {
                existing.RefCount++;
                readyObject = await BlobObjectRepository.UpdateAsync(existing, autoSave: true);
                return;
            }

            if (existing != null)
            {
                if (existing.Status == BlobObjectStatus.Deleting)
                {
                    oldDeletingKey = existing.StorageName;
                }

                existing.StorageName = BuildVersionedStorageName(sha256, GuidGenerator.Create(), blob.Suffix);
                existing.Status = BlobObjectStatus.Pending;
                existing.DeletingSince = null;
                existing.MimeType = blob.MimeType;
                await BlobObjectRepository.UpdateAsync(existing, autoSave: true);
                pendingOrReadyId = existing.Id;
                containerToWrite = existing.Container;
                storageNameToWrite = existing.StorageName;
                return;
            }

            var objId = GuidGenerator.Create();
            var storageName = BuildVersionedStorageName(sha256, objId, blob.Suffix);
            var created = new BlobObject(objId, storageScope, blob.Container, storageName, sha256, fileSize, blob.MimeType);
            await BlobObjectRepository.InsertAsync(created, autoSave: true);
            pendingOrReadyId = objId;
            containerToWrite = created.Container;
            storageNameToWrite = created.StorageName;
        });

        if (readyObject != null)
        {
            blob.SetStorageObject(null);
            blob.StorageObjectId = readyObject.Id;
            return await Repository.InsertAsync(blob, autoSave: true);
        }

        if (!string.IsNullOrWhiteSpace(oldDeletingKey))
        {
            try
            {
                await BlobContainerFactory.Create(containerToWrite).DeleteAsync(oldDeletingKey);
            }
            catch
            {
                // Best-effort cleanup of superseded versioned key.
            }
        }

        // Phase 2 (outside DB write transaction): upload physical bytes to versioned StorageName.
        await SaveBytesAsync(containerToWrite, storageNameToWrite, blob.Bytes);

        // Phase 3 (committed after upload succeeds): mark BlobObject Ready before binding logical Blob.
        var finalizedObject = await RunCommittedStepAsync(async () =>
        {
            var obj = await BlobObjectRepository.GetAsync(pendingOrReadyId);
            obj.Status = BlobObjectStatus.Ready;
            obj.RefCount = Math.Max(0, obj.RefCount) + 1;
            obj.LastVerifiedTime = Clock.Now;
            return await BlobObjectRepository.UpdateAsync(obj, autoSave: true);
        });

        blob.SetStorageObject(null);
        blob.StorageObjectId = finalizedObject.Id;
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
