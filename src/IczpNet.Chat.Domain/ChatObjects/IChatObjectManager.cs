﻿using IczpNet.AbpTrees;
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatObjects;

public interface IChatObjectManager : ITreeManager<ChatObject, long, ChatObjectInfo>
{
    Task<IQueryable<long>> QueryByKeywordAsync(string keyword);

    Task<List<long>> SearchKeywordByCacheAsync(string keyword);

    Task<ChatObject> FindByCodeAsync(string code);

    Task<ChatObject> UpdateAsync(long id, Action<ChatObject> action, bool isUnique = true);

    Task<ChatObject> UpdateAsync(ChatObject entity, Action<ChatObject> action, bool isUnique = true);

    Task<ChatObject> UpdateNameAsync(ChatObject entity, string name);

    Task<ChatObject> UpdateNameAsync(long id, string name);
    /// <summary>
    /// Group Assistant
    /// </summary>
    /// <returns></returns>
    Task<ChatObject> GetOrAddGroupAssistantAsync();

    /// <summary>
    /// Private Assistant
    /// </summary>
    /// <returns></returns>
    Task<ChatObject> GetOrAddPrivateAssistantAsync();

    Task<List<ChatObject>> GetAllListAsync(ChatObjectTypeEnums objectType);

    Task<List<ChatObject>> GetListByUserIdAsync(Guid userId);

    Task<List<long>> GetIdListByUserIdAsync(Guid userId);

    //Task<ChatObject> GetWalletAsync(Guid chatObjectId);

    //Task<ChatObjectInfo> GetItemByCacheAsync(Guid chatObjectId);

    //Task<List<ChatObjectInfo>> GetManyByCacheAsync(List<Guid> chatObjectIdList);

    //Task<List<ChatObject>> GetManyAsync(List<Guid> chatObjectIdList);

    Task<List<long>> GetIdListByNameAsync(List<string> nameList); //, List<ChatObjectTypes> objectTypes

    Task<ChatObject> CreatePersonalAsync(string name, string code, Action<ChatObject> action = null);

    Task<ChatObject> CreateShopKeeperAsync(string name, string code);

    Task<ChatObject> CreateShopWaiterAsync(long shopKeeperId, string name, string code);

    Task<ChatObject> CreateRobotAsync(string name, string code);

    Task<ChatObject> CreateSquareAsync(string name, string code);

    Task<ChatObject> CreateSubscriptionAsync(string name, string code);

    Task<ChatObject> CreateOfficialAsync(string name, string code);

    Task<ChatObject> CreateAnonymousAsync(string name, string code);

    //Task<ChatObjectInfo> GetGroupAssistantAsync();

    Task<bool> IsSomeRootAsync(params long[] idList);

    Task<ChatObject> BingAppUserIdAsync(long id, Guid appUserId);
}
