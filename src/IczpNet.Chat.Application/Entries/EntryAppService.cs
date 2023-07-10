using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectEntryValues;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Comparers;
using IczpNet.Chat.EntryNames;
using IczpNet.Chat.EntryValues;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.SessionUnitEntryValues;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Entries;

/// <summary>
/// 条目
/// </summary>
public class EntryAppService : ChatAppService, IEntryAppService
{
    protected ISessionPermissionChecker SessionPermissionChecker { get; }
    protected IRepository<EntryName, Guid> EntryNameRepository { get; }
    protected IRepository<EntryValue, Guid> EntryValueRepository { get; }
    public EntryAppService(
        ISessionPermissionChecker sessionPermissionChecker,
        IRepository<EntryName, Guid> entryNameRepository,
        IRepository<EntryValue, Guid> entryValueRepository)
    {
        SessionPermissionChecker = sessionPermissionChecker;
        EntryNameRepository = entryNameRepository;
        EntryValueRepository = entryValueRepository;
    }

    protected virtual async Task<ChatObjectDetailDto> MapToEntityDetailAsync(ChatObject entity)
    {
        await Task.Yield();
        return ObjectMapper.Map<ChatObject, ChatObjectDetailDto>(entity);
    }

    protected virtual async Task<SessionUnitDestinationDetailDto> MapToSessionUnitDetailDtoAsync(SessionUnit entity)
    {
        await Task.Yield();
        return ObjectMapper.Map<SessionUnit, SessionUnitDestinationDetailDto>(entity);
    }

    protected virtual async Task CheckEntriesInputAsync(Dictionary<Guid, List<string>> entries)
    {
        foreach (var item in entries)
        {
            var entryName = Assert.NotNull(await EntryNameRepository.FindAsync(item.Key), $"不存在EntryNameId:{item.Key}");

            Assert.If(entryName.IsRequired && item.Value.Count == 0, $"${entryName.Name}必填");

            Assert.If(item.Value.Count > entryName.MaxCount, $"${entryName.Name}最大个数：{entryName.MaxCount}");

            Assert.If(item.Value.Count < entryName.MinCount, $"${entryName.Name}最小个数：{entryName.MinCount}");

            foreach (var value in item.Value)
            {
                Assert.If(value.Length > entryName.MaxLenth, $"${entryName.Name}[{item.Value.IndexOf(value) + 1}]最大长度：{entryName.MaxLenth}");

                Assert.If(value.Length < entryName.MinLenth, $"${entryName.Name}[{item.Value.IndexOf(value) + 1}]最小长度：{entryName.MinLenth}");

                //if (entryName.IsUniqued)
                //{
                //    var isAny = !await EntryValueRepository.AnyAsync(x => x.EntryNameId == entryName.Id && x.Value == value.Value);
                //    Assert.If(isAny, $"${entryName.Name}已经存在值：{value.Value}");
                //}
            }
        }
    }

    /// <summary>
    /// CreateAsync | Update | Delete items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="second"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    protected virtual async Task<List<T>> CudItemsAsync<T>(ICollection<T> source, ICollection<T> second, IEqualityComparer<T> comparer)
    {
        //delete
        var deleteItems = source.Except(second, comparer).ToList();

        foreach (var item in deleteItems)
        {
            source.Remove(item);
        }

        //create
        var createItems = second.Except(source, comparer).ToList();

        //modify
        var modifyItems = source.Intersect(second, comparer).ToList();

        foreach (var item in modifyItems)
        {
            TryToSetLastModificationTime(item);
        }

        await Task.Yield();

        //reset
        return createItems.Concat(modifyItems).ToList();
    }

    protected virtual async Task<List<T>> FormatItemsAsync<T>(Dictionary<Guid, List<string>> entries, Func<Guid, string, Task<T>> createOrFindEntity)
    {
        var inputItems = new List<T>();

        foreach (var entry in entries)
        {
            Assert.If(!await EntryNameRepository.AnyAsync(x => x.Id == entry.Key), $"不存在EntryNameId:{entry.Key}");

            foreach (var value in entry.Value)
            {
                inputItems.Add(await createOrFindEntity(entry.Key, value));
            }
        }
        return inputItems;
    }

    /// <summary>
    /// 设置聊天对象条目
    /// </summary>
    /// <param name="ownerId">聊天对象Id</param>
    /// <param name="entries">{entryNameId:["v1","v2"]}</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ChatObjectDetailDto> SetForChatObjectAsync([Required] long ownerId, [Required] Dictionary<Guid, List<string>> entries)
    {
        var owner = await ChatObjectManager.GetAsync(ownerId);

        await CheckEntriesInputAsync(entries);

        //owner.Entries?.Clear();

        var inputItems = await FormatItemsAsync(entries, async (Guid key, string value) =>
        {
            var entryValue = await EntryValueRepository.FirstOrDefaultAsync(x => x.EntryNameId == key && x.Value == value)
                               ?? new EntryValue(GuidGenerator.Create(), key, value);
            return new ChatObjectEntryValue()
            {
                OwnerId = owner.Id,
                Owner = owner,
                EntryValue = entryValue,
                EntryValueId = entryValue.Id
            };
        });

        var entityComparer = new EntityComparer<ChatObjectEntryValue>(x => x.GetKeys().JoinAsString(","));

        owner.Entries = await CudItemsAsync(owner.Entries, inputItems, entityComparer);

        return await MapToEntityDetailAsync(owner);
    }

    /// <summary>
    /// 设置会话单元Id条目值
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="entries">{entryNameId:["v1","v2"]}</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionUnitDestinationDetailDto> SetForSessionUnitAsync([Required] Guid sessionUnitId, [Required] Dictionary<Guid, List<string>> entries)
    {
        var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

        await CheckEntriesInputAsync(entries);

        //owner.Entries?.Clear();

        var inputItems = await FormatItemsAsync(entries, async (Guid key, string value) =>
        {
            var entryValue = await EntryValueRepository.FirstOrDefaultAsync(x => x.EntryNameId == key && x.Value == value)
                               ?? new EntryValue(GuidGenerator.Create(), key, value);
            return new SessionUnitEntryValue()
            {
                SessionUnitId = sessionUnit.Id,
                SessionUnit = sessionUnit,
                EntryValue = entryValue,
                EntryValueId = entryValue.Id
            };
        });

        var entityComparer = new EntityComparer<SessionUnitEntryValue>(x => x.GetKeys().JoinAsString(","));

        sessionUnit.Entries = await CudItemsAsync(sessionUnit.Entries, inputItems, entityComparer);

        return await MapToSessionUnitDetailDtoAsync(sessionUnit);
    }

}
