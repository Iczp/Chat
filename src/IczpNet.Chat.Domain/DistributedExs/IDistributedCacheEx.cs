



using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.DistributedExs;

public interface IDistributedCacheEx<TCacheItem, TCacheKey>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<TCacheItem> GetAsync(TCacheKey key);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    Task SetAsync(TCacheKey key, TCacheItem item);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task RemoveAsync(TCacheKey key);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    Task<IDictionary<TCacheKey, TCacheItem>> GetManyAsync(IEnumerable<TCacheKey> keys);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    Task SetManyAsync(IDictionary<TCacheKey, TCacheItem> items);

    /// <summary>
    /// 批量更新（包含 Set 和 Increment）
    /// </summary>
    Task BatchUpdateAsync(IDictionary<TCacheKey, TCacheItem> updates);

    /// <summary>
    /// 批量自增 Increment 属性
    /// </summary>
    Task BatchIncrementAsync(IDictionary<TCacheKey, TCacheItem> increments);

    /// <summary>
    /// 批量更新非自增字段
    /// </summary>
    Task BatchSetFieldsAsync(IDictionary<TCacheKey, TCacheItem> updates);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    Task<IDictionary<TCacheKey, TCacheItem>> GetOrAddManyAsync(IEnumerable<TCacheKey> keys, Func<IEnumerable<TCacheKey>, Task<IDictionary<TCacheKey, TCacheItem>>> factory
);
}

