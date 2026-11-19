using System;
using System.Threading.Tasks;
using IczpNet.Chat.Commands;
using IczpNet.Chat.Enums;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 会话处置管理器实现
/// </summary>
public class SessionUnitDisposalManager(
    IRepository<SessionUnitDisposalRecord, long> disposalRepository,
    IDistributedEventBus distributedEventBus,
    IUnitOfWorkManager unitOfWorkManager) : DomainService, ISessionUnitDisposalManager
{
    protected IRepository<SessionUnitDisposalRecord, long> DisposalRepository { get; } = disposalRepository;
    protected IDistributedEventBus DistributedEventBus { get; } = distributedEventBus;
    protected IUnitOfWorkManager UnitOfWorkManager { get; } = unitOfWorkManager;

    public virtual async Task<SessionUnitDisposalRecord> RecordDisposalAsync(
        long ownerId,
        Guid sessionUnitId,
        Guid? sessionId,
        long? destinationId,
        SessionUnitDisposalType disposalType,
        SessionUnitDisposalReason reason,
        long? operatorId = null,
        string description = null)
    {
        var record = new SessionUnitDisposalRecord(
            ownerId: ownerId,
            sessionUnitId: sessionUnitId,
            sessionId: sessionId,
            destinationId: destinationId,
            disposalType: disposalType,
            reason: reason,
            operatorId: operatorId,
            description: description,
            tenantId: CurrentTenant.Id);

        var entity = await DisposalRepository.InsertAsync(record, autoSave: true);

        var eto = new SessionUnitDisposedDistributedEto
        {
            DisposalRecordId = entity.Id,
            OwnerId = entity.OwnerId,
            SessionUnitId = entity.SessionUnitId,
            SessionId = entity.SessionId,
            DestinationId = entity.DestinationId,
            DisposalType = entity.DisposalType,
            Reason = entity.Reason,
            OperatorId = entity.OperatorId,
            Description = entity.Description,
            CreationTime = entity.CreationTime
        };

        var currentUow = UnitOfWorkManager.Current;
        if (currentUow != null)
        {
            currentUow.OnCompleted(async () =>
            {
                await DistributedEventBus.PublishAsync(eto, onUnitOfWorkComplete: false, useOutbox: false);
            });
        }
        else
        {
            await DistributedEventBus.PublishAsync(eto, onUnitOfWorkComplete: false, useOutbox: false);
        }

        return entity;
    }
}
